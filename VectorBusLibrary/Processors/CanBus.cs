using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics;
using System.Threading;
using vxlapi_NET;
using static vxlapi_NET.XLDefine;

namespace VectorBusLibrary.Processors
{
    public class CanBus : CommonVector
    {
        public XL_HardwareType HardwareType { get; set; }
        public string appName { get; set; }

        public Models.CanBusRx CanBusMessageRx { get; set; } = new Models.CanBusRx();
        public string msgTestOut { get; set; }
        public CanBus(XLDriver xLDriver, XL_HardwareType xL_HardwareType, string aplicationName) : base(xLDriver, xL_HardwareType, XL_BusTypes.XL_BUS_TYPE_CAN, aplicationName)
        {
            Driver = xLDriver;
            HardwareType = xL_HardwareType;
            appName = aplicationName;
        }

        public void TestCanBus()
        {
            Trace.WriteLine("-------------------------------------------------------------------");
            Trace.WriteLine("                     VectorController                       ");
            Trace.WriteLine("");
            Trace.WriteLine("-------------------------------------------------------------------");
            Trace.WriteLine("vxlapi_NET        : " + typeof(XLDriver).Assembly.GetName().Version);

            //OpenDriver();
            GetDriverConfig();

            GetDLLVesrion();
            Trace.WriteLine(GetChannelCount());

            foreach (Models.VectorDeviceInfo item in GetListOfChannels())
            {
                Trace.WriteLine("*********************");
                Trace.WriteLine($"Channel name: {item.ChannelName}");
                Trace.WriteLine($"Channel mask: {item.ChannelMask}");
                Trace.WriteLine($"Transceiver name: {item.TransceiverName}");
                Trace.WriteLine($"Serial number: {item.SerialNumber}");

                Trace.WriteLine("---------------------");
            }

            GetAppConfigAndSetAppConfig();
            RequestTheUserToAssignChannels();
            GetAccesMask();
            Trace.WriteLine(PrintAccessMask());
            OpenPort();
            CheckPort();
            ActivateChannel();
            SetNotificationCanBus();
            ResetClock();

            RunRxThread();
            //for (int i = 0; i < 20; i++)
            //{
            //    CanTransmit();

            //}

        }

        /// <summary>
        /// Check port with function XL_CanRequestChipState
        /// </summary>
        /// <returns></returns>
        internal XL_Status CheckPort()
        {
            XL_Status status = Driver.XL_CanRequestChipState(portHandle, accessMask);
            Trace.WriteLine("Can Request Chip State: " + status);
            if (status != XL_Status.XL_SUCCESS) PrintFunctionError("CheckPort");

            return status;
        }

        /// <summary>
        /// Open port
        /// </summary>
        /// <returns></returns>
        public XLDefine.XL_Status OpenPort()
        {
            XLDefine.XL_Status status = Driver.XL_OpenPort(ref portHandle, appName, accessMask, ref permissionMask, 1024, XLDefine.XL_InterfaceVersion.XL_INTERFACE_VERSION, CommonBusType); // CanBus

            Trace.WriteLine("Open Port             : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError("OpenPort");

            return status;
        }

        /// <summary>
        /// Set notification
        /// </summary>
        /// <returns></returns>
        public XLDefine.XL_Status SetNotificationCanBus()
        {
            // Initialize EventWaitHandle object with RX event handle provided by DLL
            int tempInt = -1;
            XLDefine.XL_Status status = Driver.XL_SetNotification(portHandle, ref tempInt, 1);
            xlEvWaitHandle.SafeWaitHandle = new SafeWaitHandle(new IntPtr(tempInt), true);

            Trace.WriteLine("Set Notification      : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError("SetNotification");

            return status;
        }

        /// <summary>
        /// Transmit Can Bus message
        /// </summary>
        //public XL_Status CanTransmit(XLClass.xl_event_collection messageForTransmit)
        //{
        //    XL_Status txStatus;
        //    XLClass.xl_event_collection xlEventCollection = messageForTransmit;
        //    xlEventCollection.xlEvent[0].tagData.can_Msg.id = 0x3C0;
        //    xlEventCollection.xlEvent[0].tagData.can_Msg.dlc = 4;
        //    xlEventCollection.xlEvent[0].tagData.can_Msg.data[0] = 1;
        //    xlEventCollection.xlEvent[0].tagData.can_Msg.data[1] = 2;
        //    xlEventCollection.xlEvent[0].tagData.can_Msg.data[2] = 3;
        //    xlEventCollection.xlEvent[0].tagData.can_Msg.data[3] = 4;
        //    xlEventCollection.xlEvent[0].tagData.can_Msg.data[4] = 5;
        //    xlEventCollection.xlEvent[0].tagData.can_Msg.data[5] = 6;
        //    xlEventCollection.xlEvent[0].tagData.can_Msg.data[6] = 7;
        //    xlEventCollection.xlEvent[0].tagData.can_Msg.data[7] = 8;
        //    xlEventCollection.xlEvent[0].tag = XL_EventTags.XL_TRANSMIT_MSG;
        public void CanTransmit(XLClass.xl_event_collection messageForTransmit)
        {
            XL_Status txStatus;
            XLClass.xl_event_collection xlEventCollection = messageForTransmit;
            xlEventCollection.xlEvent[0].tagData.can_Msg.id = 0x3C0;
            xlEventCollection.xlEvent[0].tagData.can_Msg.dlc = 4;
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[0] = 1;
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[1] = 2;
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[2] = 3;
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[3] = 4;
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[4] = 5;
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[5] = 6;
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[6] = 7;
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[7] = 8;
            xlEventCollection.xlEvent[0].tag = XL_EventTags.XL_TRANSMIT_MSG;

            txStatus = Driver.XL_CanTransmit(portHandle, txMask, xlEventCollection);
            Trace.WriteLine("Transmit Message      : " + txStatus);
            return txStatus;
        }


        /// <summary>
        /// Run Rx Thread
        /// </summary>
        public void RunRxThread()
        {
            Trace.WriteLine("Start Rx thread...");
            rxThread = new Thread(new ThreadStart(RXThread));
            rxThread.Name = "CanBusRxThread";
            rxThread.Start();
        }

        /// <summary>
        /// RX thread with funcion xlReceive and xlGetEventString
        /// </summary>
        private void RXThread()
        {
            // Create new object containing received data 
            XLClass.xl_event receivedEvent = new();

            // Result of XL Driver function calls
            XL_Status xlStatus = XL_Status.XL_SUCCESS;

            // Note: this thread will be destroyed by MAIN
            while (true)
            {
                // Wait for hardware events
                if (xlEvWaitHandle.WaitOne(1000))
                {
                    // ...init xlStatus first
                    xlStatus = XL_Status.XL_SUCCESS;

                    // afterwards: while hw queue is not empty...
                    while (xlStatus != XL_Status.XL_ERR_QUEUE_IS_EMPTY)
                    {
                        // ...block RX thread to generate RX-Queue overflows
                        while (blockRxThread) { Thread.Sleep(1000); }

                        // ...receive data from hardware.
                        xlStatus = Driver.XL_Receive(portHandle, ref receivedEvent);

                        //  If receiving succeed....
                        if (xlStatus == XL_Status.XL_SUCCESS)
                        {
                            if ((receivedEvent.flags & XL_MessageFlags.XL_EVENT_FLAG_OVERRUN) != 0)
                            {
                                Trace.WriteLine("-- XL_EVENT_FLAG_OVERRUN --");
                            }

                            // ...and data is a Rx msg...
                            if (receivedEvent.tag == XL_EventTags.XL_RECEIVE_MSG)
                            {
                                string preString = $"Flags: {receivedEvent.flags} - ID: {receivedEvent.tagData.can_Msg.id} - Data: {receivedEvent.tagData.can_Msg.data[0]}*{receivedEvent.tagData.can_Msg.data[1]}*{receivedEvent.tagData.can_Msg.data[2]} -- ROW[{Driver.XL_GetEventString(receivedEvent)}]";

                                Trace.WriteLine(preString);


                                if ((receivedEvent.tagData.can_Msg.flags & XL_MessageFlags.XL_CAN_MSG_FLAG_OVERRUN) != 0)
                                {
                                    Trace.WriteLine("-- XL_CAN_MSG_FLAG_OVERRUN --");
                                }

                                // ...check various flags
                                if ((receivedEvent.tagData.can_Msg.flags & XL_MessageFlags.XL_CAN_MSG_FLAG_ERROR_FRAME)
                                    == XL_MessageFlags.XL_CAN_MSG_FLAG_ERROR_FRAME)
                                {
                                    Trace.WriteLine("ERROR FRAME");
                                }

                                else if ((receivedEvent.tagData.can_Msg.flags & XL_MessageFlags.XL_CAN_MSG_FLAG_REMOTE_FRAME)
                                    == XL_MessageFlags.XL_CAN_MSG_FLAG_REMOTE_FRAME)
                                {
                                    Trace.WriteLine("REMOTE FRAME");
                                }

                                else
                                {
                                    CanBusMessageRx.TimeStamp = receivedEvent.timeStamp;
                                    CanBusMessageRx.MessageId = receivedEvent.tagData.can_Msg.id;
                                    CanBusMessageRx.DLC = receivedEvent.tagData.can_Msg.dlc;
                                    CanBusMessageRx.data = receivedEvent.tagData.can_Msg.data;
                                    CanBusMessageRx.RawCanMessage = Driver.XL_GetEventString(receivedEvent);

                                    Trace.WriteLine("OK MSG");

                                }
                            }
                        }
                    }
                }
                // No event occurred
            }
        }




        // -----------------------------------------------------------------------------------------------
        /// <summary>
        /// Retrieve the application channel assignment and test if this channel can be opened
        /// </summary>
        // -----------------------------------------------------------------------------------------------
        public bool GetAppChannelAndTestIsOk(uint appChIdx, ref ulong chMask, ref int chIdx)
        {
            XL_Status status = Driver.XL_GetApplConfig(appName, appChIdx, ref hwType, ref hwIndex, ref hwChannel, CommonBusType);
            if (status != XL_Status.XL_SUCCESS)
            {
                Trace.WriteLine("XL_GetApplConfig      : " + status);
                PrintFunctionError("GetAppChannelAndTestIsOk");
            }

            chMask = Driver.XL_GetChannelMask(hwType, (int)hwIndex, (int)hwChannel);
            chIdx = Driver.XL_GetChannelIndex(hwType, (int)hwIndex, (int)hwChannel);
            Trace.WriteLine($"***************** TxMask:{txMask} - RxMask:{rxMask} - AcsMask:{accessMask} (GetAppChannelAndTestIsOk)");
            if (chIdx < 0 || chIdx >= driverConfig.channelCount)
            {
                // the (hwType, hwIndex, hwChannel) triplet stored in the application configuration does not refer to any available channel.
                return false;
            }

            // test if CAN is available on this channel
            return (driverConfig.channel[chIdx].channelBusCapabilities & XL_BusCapabilities.XL_BUS_ACTIVE_CAP_CAN) != 0;
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

    }
}
