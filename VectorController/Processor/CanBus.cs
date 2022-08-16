using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using vxlapi_NET;
using static vxlapi_NET.XLDefine;

namespace VectorController.Processor
{
    internal class CanBus : CommonVector
    {
        public XL_HardwareType hardwareType { get; set; }


        public CanBus(XLDriver xLDriver, XL_HardwareType xL_HardwareType) : base(xLDriver, xL_HardwareType, XL_BusTypes.XL_BUS_TYPE_CAN)
        {
            Driver = xLDriver;
            hardwareType = xL_HardwareType;
        }

        public void TestCanBus() 
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

                Trace.WriteLine("---------------------");
            }

            GetAppConfigAndSetAppConfig();
            RequestTheUserToAssignChannels();
            GetAccesMask();
            PrintAccessMask();
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
        internal XLDefine.XL_Status CheckPort()
        {
            XLDefine.XL_Status status = base.Driver.XL_CanRequestChipState(portHandle, accessMask);
            Trace.WriteLine("Can Request Chip State: " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError("CheckPort");

            return status;
        }


        /// <summary>
        /// Transmit Can Bus message
        /// </summary>
        internal void CanTransmit()
        {
            XLDefine.XL_Status txStatus;
            XLClass.xl_event_collection xlEventCollection = new(1);
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
            xlEventCollection.xlEvent[0].tag = XLDefine.XL_EventTags.XL_TRANSMIT_MSG;

            txStatus = Driver.XL_CanTransmit(portHandle, txMask, xlEventCollection);
            Trace.WriteLine("Transmit Message      : " + txStatus);
        }


        /// <summary>
        /// Run Rx Thread
        /// </summary>
        private void RunRxThread()
        {
            Trace.WriteLine("Start Rx thread...");
            rxThreadDDD = new Thread(new ThreadStart(RXThread));
            rxThreadDDD.Start();
        }

        /// <summary>
        /// RX thread with funcion xlReceive and xlGetEventString
        /// </summary>
        private void RXThread()
        {
            // Create new object containing received data 
            XLClass.xl_event receivedEvent = new XLClass.xl_event();

            // Result of XL Driver function calls
            XLDefine.XL_Status xlStatus = XLDefine.XL_Status.XL_SUCCESS;


            // Note: this thread will be destroyed by MAIN
            while (true)
            {
                // Wait for hardware events
                if (xlEvWaitHandle.WaitOne(1000))
                {
                    // ...init xlStatus first
                    xlStatus = XLDefine.XL_Status.XL_SUCCESS;

                    // afterwards: while hw queue is not empty...
                    while (xlStatus != XLDefine.XL_Status.XL_ERR_QUEUE_IS_EMPTY)
                    {
                        // ...block RX thread to generate RX-Queue overflows
                        while (blockRxThread) { Thread.Sleep(1000); }

                        // ...receive data from hardware.
                        xlStatus = Driver.XL_Receive(portHandle, ref receivedEvent);

                        //  If receiving succeed....
                        if (xlStatus == XLDefine.XL_Status.XL_SUCCESS)
                        {
                            if ((receivedEvent.flags & XLDefine.XL_MessageFlags.XL_EVENT_FLAG_OVERRUN) != 0)
                            {
                                Trace.WriteLine("-- XL_EVENT_FLAG_OVERRUN --");
                            }

                            // ...and data is a Rx msg...
                            if (receivedEvent.tag == XLDefine.XL_EventTags.XL_RECEIVE_MSG)
                            {
                                string preString = $"Flags: {receivedEvent.flags} - ID: {receivedEvent.tagData.can_Msg.id} - Data: {receivedEvent.tagData.can_Msg.data[0]}*{receivedEvent.tagData.can_Msg.data[1]}*{receivedEvent.tagData.can_Msg.data[2]} -- ROW[{Driver.XL_GetEventString(receivedEvent)}]";

                                Trace.WriteLine(preString);


                                if ((receivedEvent.tagData.can_Msg.flags & XLDefine.XL_MessageFlags.XL_CAN_MSG_FLAG_OVERRUN) != 0)
                                {
                                    Trace.WriteLine("-- XL_CAN_MSG_FLAG_OVERRUN --");
                                }

                                // ...check various flags
                                if ((receivedEvent.tagData.can_Msg.flags & XLDefine.XL_MessageFlags.XL_CAN_MSG_FLAG_ERROR_FRAME)
                                    == XLDefine.XL_MessageFlags.XL_CAN_MSG_FLAG_ERROR_FRAME)
                                {
                                    Trace.WriteLine("ERROR FRAME");
                                }

                                else if ((receivedEvent.tagData.can_Msg.flags & XLDefine.XL_MessageFlags.XL_CAN_MSG_FLAG_REMOTE_FRAME)
                                    == XLDefine.XL_MessageFlags.XL_CAN_MSG_FLAG_REMOTE_FRAME)
                                {
                                    Trace.WriteLine("REMOTE FRAME");
                                }

                                else
                                {
                                    Trace.WriteLine(Driver.XL_GetEventString(receivedEvent));
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
        internal bool GetAppChannelAndTestIsOk(uint appChIdx, ref UInt64 chMask, ref int chIdx)
        {
            XLDefine.XL_Status status = Driver.XL_GetApplConfig(appName, appChIdx, ref hwType, ref hwIndex, ref hwChannel, CommonBusType);
            if (status != XLDefine.XL_Status.XL_SUCCESS)
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
            return (driverConfig.channel[chIdx].channelBusCapabilities & XLDefine.XL_BusCapabilities.XL_BUS_ACTIVE_CAP_CAN) != 0;
        }

        /// <summary>
        /// Request the user to assign channels until both CAN1 (Tx) and CAN2 (Rx) are assigned to usable channels
        /// </summary>
        internal void RequestTheUserToAssignChannels()
        {
            if (!GetAppChannelAndTestIsOk(0, ref txMask, ref txCi) || !GetAppChannelAndTestIsOk(1, ref rxMask, ref rxCi))
            {
                PrintAssignErrorAndPopupHwConf();
            }
        }

    }
}
