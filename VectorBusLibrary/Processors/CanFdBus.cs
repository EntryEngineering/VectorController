using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using vxlapi_NET;
using static vxlapi_NET.XLDefine;

namespace VectorBusLibrary.Processors
{
    public class CanFdBus : CommonVector
    {

        // -----------------------------------------------------------------------------------------------
        // DLL Import for RX events
        // -----------------------------------------------------------------------------------------------
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int WaitForSingleObject(int handle, int timeOut);
        // -----------------------------------------------------------------------------------------------

        public XL_HardwareType HardwareType { get; set; }
        public Models.CanFdMessageModelRx CanFdBusMessageRx { get; set; } = new Models.CanFdMessageModelRx();
        public string appName { get; set; }

        private static readonly uint canFdModeNoIso = 0;      // Global CAN FD ISO (default) / no ISO mode flag

        private static int eventHandle = -1;

        public CanFdBus(XL_HardwareType xL_HardwareType, string aplicationName) : base(xL_HardwareType, XL_BusTypes.XL_BUS_TYPE_CAN, aplicationName)
        {
            Driver = new XLDriver();
            HardwareType = xL_HardwareType;
            appName = aplicationName;
        }

        [STAThread]
        public void TestCanFDBus()
        {
            Trace.WriteLine("-------------------------------------------------------------------");
            Trace.WriteLine("                     VectorController                       ");
            Trace.WriteLine("");
            Trace.WriteLine("-------------------------------------------------------------------");
            Trace.WriteLine("vxlapi_NET        : " + typeof(XLDriver).Assembly.GetName().Version);

            OpenDriver();
            GetDriverConfig();


            GetDLLVesrion();
            Trace.WriteLine(GetChannelCount());

            foreach (var item in GetListOfChannels())
            {
                Trace.WriteLine("*********************");
                Trace.WriteLine($"Channel name: {item.ChannelName}");
                Trace.WriteLine($"Channel mask: {item.ChannelMask}");
                Trace.WriteLine($"Transceiver name: {item.TransceiverName}");
                Trace.WriteLine($"Serial number: {item.SerialNumber}");
                Trace.WriteLine($"Channel compatible CanFD: {item.CanFdCompatible}");

                Trace.WriteLine("---------------------");
            }

            GetAppConfigAndSetAppConfig();
            RequestTheUserToAssignChannels();
            PrintConfig();
            GetAccesMask();
            PrintAccessMask();
            OpenPort();
            SetCanFdConfiguration();
            SetNotificationCanFdBus();
            ActivateChannel();
            GetXlDriverConfiguration();

            RunRxThread();

        }

        /// <summary>
        /// Set notification CanFd Bus
        /// </summary>
        /// <returns></returns>
        public XL_Status SetNotificationCanFdBus()
        {
            XL_Status status = Driver.XL_SetNotification(portHandle, ref eventHandle, 1);
            Trace.WriteLine("Set Notification      : " + status);
            if (status != XL_Status.XL_SUCCESS) PrintFunctionError("Get RX event handle");

            return status;
        }

        /// <summary>
        /// Set CanFd configuration
        /// </summary>
        /// <returns></returns>
        public XL_Status SetCanFdConfiguration()
        {
            XLClass.XLcanFdConf canFdConf = new();

            // arbitration bitrate
            canFdConf.arbitrationBitRate = 500000;  //Arbitration CAN bus timing for nominal / arbitration bit rate in bit/s.
            canFdConf.tseg1Abr = 6;  //Arbitration CAN bus timing tseg1.Range: 1 < tseg1Abr < 255.
            canFdConf.tseg2Abr = 3;  //Arbitration CAN bus timing tseg2.Range: 1 < tseg2Abr < 255.
            canFdConf.sjwAbr = 2; // Arbitration CAN bus timing value (sample jump width).Range: 0 < sjwAbr <= min(tseg2Abr, 128).

            // data bitrate
            canFdConf.dataBitRate = canFdConf.arbitrationBitRate * 4;  // CAN bus timing for data bit rate in bit/s.Range: dataBitRate >= max(arbitrationBitRate, 25000).
            canFdConf.tseg1Dbr = 6;  //Data phase CAN bus timing for data tseg1.Range: 1 < tseg1Dbr < 127.
            canFdConf.tseg2Dbr = 3;  //Data phase CAN bus timing for data tseg2.Range: 1 < tseg2Dbr < 127.
            canFdConf.sjwDbr = 2;  // Arbitration CAN bus timing value (sample jump width).Range: 0 < sjwAbr <= min(tseg2Abr, 128).

            if (canFdModeNoIso > 0)
            {
                canFdConf.options = (byte)XL_CANFD_ConfigOptions.XL_CANFD_CONFOPT_NO_ISO;
            }
            else
            {
                canFdConf.options = 0;
            }

            XL_Status status = Driver.XL_CanFdSetConfiguration(portHandle, accessMask, canFdConf);
            Trace.WriteLine("Set CAN FD Config     : " + status);
            if (status != XL_Status.XL_SUCCESS) PrintFunctionError("SetCanFdConfiguration");

            return status;
        }

        /// <summary>
        /// Get XL Driver configuration
        /// </summary>
        /// <returns></returns>
        public XL_Status GetXlDriverConfiguration()
        {
            // Get XL Driver configuration to get the actual setup parameter
            XL_Status status = Driver.XL_GetDriverConfig(ref driverConfig);
            if (status != XL_Status.XL_SUCCESS) PrintFunctionError("GetXlDriverConfiguration");

            if (canFdModeNoIso > 0)
            {
                Trace.WriteLine("CAN FD mode           : NO ISO");
            }
            else
            {
                Trace.WriteLine("CAN FD mode           : ISO");
            }
            Trace.WriteLine("TX Arb. BitRate       : " + driverConfig.channel[txCi].busParams.dataCanFd.arbitrationBitRate
                            + "Bd, Data Bitrate: " + driverConfig.channel[txCi].busParams.dataCanFd.dataBitRate + "Bd");

            return status;
        }

        /// <summary>
        /// Open port
        /// </summary>
        /// <returns></returns>
        public XLDefine.XL_Status OpenPort()
        {

            XLDefine.XL_Status status = Driver.XL_OpenPort(ref portHandle, appName, accessMask, ref permissionMask, 16000, XLDefine.XL_InterfaceVersion.XL_INTERFACE_VERSION_V4, CommonBusType); //  CanFdBus
            Trace.WriteLine("Open Port             : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError("OpenPort");

            return status;
        }


        /// <summary>
        /// Run Rx Thread
        /// </summary>
        public void RunRxThread()
        {
            Trace.WriteLine("Start Rx thread...");
            rxThread = new Thread(new ThreadStart(RXThread));
            rxThread.Start();
        }

        /// <summary>
        /// Receive thread
        /// </summary>
        public void RXThread()
        {
            // Create new object containing received data 
            XLClass.XLcanRxEvent receivedEvent = new();

            // Result of XL Driver function calls
            XL_Status xlStatus = XL_Status.XL_SUCCESS;

            // Result values of WaitForSingleObject 
            WaitResults waitResult = new();


            // Note: this thread will be destroyed by MAIN
            while (true)
            {
                // Wait for hardware events
                waitResult = (WaitResults)WaitForSingleObject(eventHandle, 1000);

                // If event occurred...
                if (waitResult != WaitResults.WAIT_TIMEOUT)
                {
                    // ...init xlStatus first
                    xlStatus = XL_Status.XL_SUCCESS;

                    // afterwards: while hw queue is not empty...
                    while (xlStatus != XL_Status.XL_ERR_QUEUE_IS_EMPTY)
                    {
                        // ...block RX thread to generate RX-Queue overflows
                        while (blockRxThread) Thread.Sleep(1000);

                        // ...receive data from hardware.
                        xlStatus = Driver.XL_CanReceive(portHandle, ref receivedEvent);

                        //  If receiving succeed....
                        if (xlStatus == XL_Status.XL_SUCCESS)
                        {
                            Trace.WriteLine(Driver.XL_CanGetEventString(receivedEvent));


                            CanFdBusMessageRx.canId = receivedEvent.tagData.canRxOkMsg.canId;
                            CanFdBusMessageRx.msgFlags = ((uint)receivedEvent.tagData.canRxOkMsg.msgFlags);
                            CanFdBusMessageRx.crc = receivedEvent.tagData.canRxOkMsg.crc;
                            CanFdBusMessageRx.reserved1 = receivedEvent.reserved1;
                            CanFdBusMessageRx.totalBitCnt = receivedEvent.tagData.canRxOkMsg.totalBitCnt;
                            CanFdBusMessageRx.dlc = receivedEvent.tagData.canRxOkMsg.dlc;
                            CanFdBusMessageRx.reserved = receivedEvent.reserved;
                            CanFdBusMessageRx.data = receivedEvent.tagData.canRxOkMsg.data;
                            CanFdBusMessageRx.RawDataString = Driver.XL_CanGetEventString(receivedEvent);


                        }
                    }
                }
            }
        }

        /// <summary>
        /// CanFd transmit sample
        /// </summary>
        public void CanFdTransmit(XLClass.xl_canfd_event_collection messageForTransmit)
        {
            XL_Status txStatus;

            XLClass.xl_canfd_event_collection xlEventCollection = messageForTransmit;

            xlEventCollection.xlCANFDEvent[0].tag = XL_CANFD_TX_EventTags.XL_CAN_EV_TAG_TX_MSG;
            xlEventCollection.xlCANFDEvent[0].tagData.canId = 0x100;
            xlEventCollection.xlCANFDEvent[0].tagData.dlc = XL_CANFD_DLC.DLC_CAN_CANFD_8_BYTES;
            xlEventCollection.xlCANFDEvent[0].tagData.msgFlags = XL_CANFD_TX_MessageFlags.XL_CAN_TXMSG_FLAG_BRS | XL_CANFD_TX_MessageFlags.XL_CAN_TXMSG_FLAG_EDL;
            xlEventCollection.xlCANFDEvent[0].tagData.data[0] = 1;
            xlEventCollection.xlCANFDEvent[0].tagData.data[1] = 1;
            xlEventCollection.xlCANFDEvent[0].tagData.data[2] = 2;
            xlEventCollection.xlCANFDEvent[0].tagData.data[3] = 2;
            xlEventCollection.xlCANFDEvent[0].tagData.data[4] = 3;
            xlEventCollection.xlCANFDEvent[0].tagData.data[5] = 3;
            xlEventCollection.xlCANFDEvent[0].tagData.data[6] = 4;
            xlEventCollection.xlCANFDEvent[0].tagData.data[7] = 4;

            uint messageCounterSent = 0;
            txStatus = Driver.XL_CanTransmitEx(portHandle, txMask, ref messageCounterSent, xlEventCollection);
            Trace.WriteLine($"Transmit Message      : {txStatus} {messageCounterSent}");
        }



        /// <summary>
        /// Request the user to assign channels until both CAN1 (Tx) and CAN2 (Rx) are assigned to usable channels
        /// </summary>
        public void RequestTheUserToAssignChannels()
        {
            if (!GetAppChannelAndTestIsOk(0, ref txMask, ref txCi) || !GetAppChannelAndTestIsOk(1, ref rxMask, ref rxCi))
            {
                PrintAssignErrorAndPopupHwConf();
            }
        }

        // -----------------------------------------------------------------------------------------------
        /// <summary>
        /// Retrieve the application channel assignment and test if this channel can be opened
        /// </summary>
        // -----------------------------------------------------------------------------------------------
        public bool GetAppChannelAndTestIsOk(uint appChIdx, ref ulong chMask, ref int chIdx)
        {
            XL_Status status = Driver.XL_GetApplConfig(appName, appChIdx, ref hwType, ref hwIndex, ref hwChannel, XL_BusTypes.XL_BUS_TYPE_CAN);
            if (status != XL_Status.XL_SUCCESS)
            {
                Trace.WriteLine("XL_GetApplConfig      : " + status);
                PrintFunctionError("GetAppChannelAndTestIsOk");
            }

            chMask = Driver.XL_GetChannelMask(hwType, (int)hwIndex, (int)hwChannel);
            chIdx = Driver.XL_GetChannelIndex(hwType, (int)hwIndex, (int)hwChannel);
            if (chIdx < 0 || chIdx >= driverConfig.channelCount)
            {
                // the (hwType, hwIndex, hwChannel) triplet stored in the application configuration does not refer to any available channel.
                return false;
            }

            if ((driverConfig.channel[chIdx].channelBusCapabilities & XL_BusCapabilities.XL_BUS_ACTIVE_CAP_CAN) == 0)
            {
                // CAN is not available on this channel
                return false;
            }

            if (canFdModeNoIso > 0)
            {
                if ((driverConfig.channel[chIdx].channelCapabilities & XL_ChannelCapabilities.XL_CHANNEL_FLAG_CANFD_BOSCH_SUPPORT) == 0)
                {
                    Trace.WriteLine($"{driverConfig.channel[chIdx].name.TrimEnd(' ', '\0')} {driverConfig.channel[chIdx].transceiverName.TrimEnd(' ', '\0')} does not support CAN FD NO-ISO");
                    return false;
                }
            }
            else
            {
                if ((driverConfig.channel[chIdx].channelCapabilities & XL_ChannelCapabilities.XL_CHANNEL_FLAG_CANFD_ISO_SUPPORT) == 0)
                {
                    Trace.WriteLine($"{driverConfig.channel[chIdx].name.TrimEnd(' ', '\0')} {driverConfig.channel[chIdx].transceiverName.TrimEnd(' ', '\0')} does not support CAN FD ISO");
                    return false;
                }
            }

            return true;
        }



    }
}
