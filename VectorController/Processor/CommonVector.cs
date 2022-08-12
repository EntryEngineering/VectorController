using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Timers;
using VectorController.Models;
using vxlapi_NET;

namespace VectorController.Processor
{
    internal partial class CommonVector
    {
        // -----------------------------------------------------------------------------------------------
        // Global variables
        // -----------------------------------------------------------------------------------------------
        // Driver access through XLDriver (wrapper)

        internal XLDriver driver { get; set; }
        internal XLDefine.XL_BusTypes commonBusType { get; set; }
        protected static string appName = "TestVectorControllerV1";

        // Driver configuration
        private static XLClass.xl_driver_config driverConfig = new XLClass.xl_driver_config();

        // Variables required by XLDriver
        protected static XLDefine.XL_HardwareType hwType;
        protected static uint hwIndex = 1;
        protected static uint hwChannel = 1;
        protected static int portHandle = -1;
        protected static UInt64 accessMask = 0;
        protected static UInt64 permissionMask = 0;
        protected static UInt64 txMask = 0;
        protected static UInt64 rxMask = 0;
        protected static int txCi = -1;
        protected static int rxCi = -1;
        protected static EventWaitHandle xlEvWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, null);

        // RX thread
        protected static Thread rxThreadDDD;
        protected static bool blockRxThread = false;
        private XLDriver xLDriver;
        private XLDefine.XL_HardwareType xL_HardwareType;

        // -----------------------------------------------------------------------------------------------

        public CommonVector(XLDriver xLDriver, XLDefine.XL_HardwareType xL_HardwareType,XLDefine.XL_BusTypes busType)
        {
            driver = xLDriver;
            hwType = xL_HardwareType;
            commonBusType = busType;
        }





        //*******************************
        //**** Common CAN,CANFD and Lin Bus API below 
        //*******************************



        /// <summary>
        /// Open driver
        /// </summary>
        /// <returns>XLDefine.XL_Status</returns>
        public XLDefine.XL_Status OpenDriver()
        {
            XLDefine.XL_Status status = driver.XL_OpenDriver();
            Trace.WriteLine("Open Driver       : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError("OpenDriver");
            return status;
        }

        /// <summary>
        /// Close driver
        /// </summary>
        /// <returns></returns>
        internal XLDefine.XL_Status CloseDriver()
        {
            XLDefine.XL_Status status = driver.XL_CloseDriver();
            Trace.WriteLine("Close driver          : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError("CloseDriver");

            return status;
        }

        /// <summary>
        /// Get and Set app config
        /// </summary>
        internal void GetAppConfigAndSetAppConfig()
        {
            // If the application name cannot be found in VCANCONF..
            if ((driver.XL_GetApplConfig(appName, 0, ref hwType, ref hwIndex, ref hwChannel, commonBusType) != XLDefine.XL_Status.XL_SUCCESS) ||
                (driver.XL_GetApplConfig(appName, 1, ref hwType, ref hwIndex, ref hwChannel, commonBusType) != XLDefine.XL_Status.XL_SUCCESS))
            {
                //...create the item with two CAN channels
                driver.XL_SetApplConfig(appName, 0, XLDefine.XL_HardwareType.XL_HWTYPE_NONE, 0, 0, commonBusType);
                driver.XL_SetApplConfig(appName, 1, XLDefine.XL_HardwareType.XL_HWTYPE_NONE, 0, 0, commonBusType);
                PrintAssignErrorAndPopupHwConf();
            }
        }

        /// <summary>
        /// Get driver config
        /// </summary>
        /// <returns>XLDefine.XL_Status</returns>
        public XLDefine.XL_Status GetDriverConfig()
        {
            XLDefine.XL_Status status = driver.XL_GetDriverConfig(ref driverConfig);
            Trace.WriteLine("Get Driver Config : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError("GetDriverConfig");
            return status;
        }



        // xlGetRemoteDriverConfig - not use
        // xlGetChannelIndex - not use
        // xlGetChannelMask - TODO


        /// <summary>
        /// Open port
        /// </summary>
        /// <returns></returns>
        internal XLDefine.XL_Status OpenPort()
        {
            XLDefine.XL_Status status = driver.XL_OpenPort(ref portHandle, appName, accessMask, ref permissionMask, 1024, XLDefine.XL_InterfaceVersion.XL_INTERFACE_VERSION, commonBusType);
            Trace.WriteLine("Open Port             : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError("OpenPort");

            return status;
        }

        /// <summary>
        /// Close port
        /// </summary>
        /// <returns></returns>
        internal XLDefine.XL_Status ClosePort()
        {
            XLDefine.XL_Status status = driver.XL_ClosePort(portHandle);
            Trace.WriteLine("Close port          : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError("ClosePort");

            return status;
        }

        /// <summary>
        /// Set time rate
        /// </summary>
        /// <param name="timeRate"></param>
        /// <returns></returns>
        internal XLDefine.XL_Status SetTimeRate(uint timeRate)
        {
            XLDefine.XL_Status status = driver.XL_SetTimerRate(portHandle, timeRate);
            Trace.WriteLine("Time rate was set           : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError("DeactivateChannle");

            return status;
        }


        // xlSetTimerRateAndChannel - not use

        /// <summary>
        /// Reset time stamp clock 
        /// </summary>
        /// <returns></returns>
        internal XLDefine.XL_Status ResetClock()
        {
            XLDefine.XL_Status status = driver.XL_ResetClock(portHandle);
            Trace.WriteLine("Reset Clock           : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError("ResetClock");

            return status;
        }

        /// <summary>
        /// Set notification
        /// </summary>
        /// <returns></returns>
        internal XLDefine.XL_Status SetNotification()
        {
            // Initialize EventWaitHandle object with RX event handle provided by DLL
            int tempInt = -1;
            XLDefine.XL_Status status = driver.XL_SetNotification(portHandle, ref tempInt, 1);
            xlEvWaitHandle.SafeWaitHandle = new SafeWaitHandle(new IntPtr(tempInt), true);

            Trace.WriteLine("Set Notification      : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError("SetNotification");

            return status;
        }

        /// <summary>
        /// Flush receive queue
        /// </summary>
        /// <returns></returns>
        internal XLDefine.XL_Status FlushReceiveQueue()
        {
            XLDefine.XL_Status status = driver.XL_FlushReceiveQueue(portHandle);
            Trace.WriteLine("Flush Receive Queue          : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError("FlushReceiveQueue");

            return status;
        }


        // xlGetReceiveQueueLevel - not use

        /// <summary>
        /// Activate channel
        /// </summary>
        /// <returns></returns>
        internal XLDefine.XL_Status ActivateChannel()
        {
            XLDefine.XL_Status status = driver.XL_ActivateChannel(portHandle, accessMask, commonBusType, XLDefine.XL_AC_Flags.XL_ACTIVATE_NONE);
            Trace.WriteLine("Activate Channel      : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError("ActivateChannel");

            return status;
        }


        /// <summary>
        /// Run Rx Thread
        /// </summary>
        internal void RunRxThread()
        {
            Trace.WriteLine("Start Rx thread...");
            rxThreadDDD = new Thread(new ThreadStart(RXThread));
            rxThreadDDD.Start();
        }

        /// <summary>
        /// RX thread with funcion xlReceive and xlGetEventString
        /// </summary>
        internal void RXThread()
        {
            // Create new object containing received data 
            XLClass.xl_event receivedEvent = new XLClass.xl_event();

            // Result of XL Driver function calls
            XLDefine.XL_Status xlStatus = XLDefine.XL_Status.XL_SUCCESS;


            // Note: this thread will be destroyed by MAIN
            while (true)
            {
                //Console.WriteLine(i + "  Wait for hardware events");
                //i = i+1;
                //if (i >= 10)
                //{
                //    xlStatus = XLDefine.XL_Status.XL_ERR_CONNECTION_CLOSED;
                //}
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
                        xlStatus = driver.XL_Receive(portHandle, ref receivedEvent);

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
                                string preString = $"Flags: {receivedEvent.flags} - ID: {receivedEvent.tagData.can_Msg.id} - Data: {receivedEvent.tagData.can_Msg.data[0]}*{receivedEvent.tagData.can_Msg.data[1]}*{receivedEvent.tagData.can_Msg.data[2]} -- ROW[{driver.XL_GetEventString(receivedEvent)}]";


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
                                    //Console.WriteLine(CANDemo.XL_GetEventString(receivedEvent));
                                    Trace.WriteLine("OK MSG");
                                }
                            }
                        }
                    }
                }
                // No event occurred
            }
        }

        // xlGetErrorString - not use
        // xlGetSyncTime - TODO
        // xlGetChannelTime - TODO
        // xlGenerateSyncPulse - TODO

        /// <summary>
        /// xlPopupHwConfig for assigned device with XLDriver
        /// </summary>
        internal void PrintAssignErrorAndPopupHwConf()
        {
            Trace.WriteLine("Please check application settings of " + appName + " CAN1/CAN2,assign them to available hardware channels and press enter.");
            driver.XL_PopupHwConfig();
        }

        /// <summary>
        /// Deactivate channel
        /// </summary>
        /// <returns></returns>
        internal XLDefine.XL_Status DeactivateChannle()
        {
            XLDefine.XL_Status status = driver.XL_DeactivateChannel(portHandle, accessMask);
            Trace.WriteLine("Deactivate channel          : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError("DeactivateChannle");

            return status;
        }

        // xlGetLicenseInfo - not use
        // xlSetGlobalTimeSync - not use
        // xlGetKeymanBoxes - not use
        // xlGetKeymanInfo - not use
        // xlCreateDriverConfig - TODO
        // xlDestroyDriverConfig - TODO




        // -----------------------------------------------------------------------------------------------
        /// <summary>
        /// Retrieve the application channel assignment and test if this channel can be opened
        /// </summary>
        // -----------------------------------------------------------------------------------------------
        internal bool GetAppChannelAndTestIsOk(uint appChIdx, ref UInt64 chMask, ref int chIdx)
        {
            XLDefine.XL_Status status = driver.XL_GetApplConfig(appName, appChIdx, ref hwType, ref hwIndex, ref hwChannel, commonBusType);
            if (status != XLDefine.XL_Status.XL_SUCCESS)
            {
                Trace.WriteLine("XL_GetApplConfig      : " + status);
                PrintFunctionError("GetAppChannelAndTestIsOk");
            }

            chMask = driver.XL_GetChannelMask(hwType, (int)hwIndex, (int)hwChannel);
            chIdx = driver.XL_GetChannelIndex(hwType, (int)hwIndex, (int)hwChannel);
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
        /// Get DLL verion of XL Driver
        /// </summary>
        /// <returns></returns>
        public string GetDLLVesrion()
        {
            return driver.VersionToString(driverConfig.dllVersion);
        }


        /// <summary>
        /// Get chnnel count
        /// </summary>
        /// <returns></returns>
        public uint GetChannelCount()
        {
            return driverConfig.channelCount;
        }

        /// <summary>
        /// Get list of channels
        /// </summary>
        /// <returns></returns>
        internal List<VectorDeviceInfo> GetListOfChannels()
        {
            List<VectorDeviceInfo> list = new();
            for (int i = 0; i < driverConfig.channelCount; i++)
            {
                list.Add(new VectorDeviceInfo()
                {
                    ChannelName = driverConfig.channel[i].name,
                    ChannelMask = driverConfig.channel[i].channelMask,
                    TransceiverName = driverConfig.channel[i].transceiverName,
                    SerialNumber = driverConfig.channel[i].serialNumber
                });
            }
            return list;
        }


        /// <summary>
        /// Get access mask
        /// </summary>
        internal void GetAccesMask()
        {
            accessMask = txMask | rxMask;
            permissionMask = accessMask;
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


        /// <summary>
        /// Print access mask in debug console
        /// </summary>
        internal void PrintAccessMask()
        {
            Trace.WriteLine($"PrintAccessMask >> TxMask:{txMask} - RxMask:{rxMask} - AcsMask:{accessMask} po accessMask = txMask | rxMask");
        }

        /// <summary>
        /// Print function error
        /// </summary>
        /// <param name="functionName"></param>
        /// <returns></returns>
        internal int PrintFunctionError(string functionName)
        {
            Trace.WriteLine($"ERROR: Function {functionName} call failed!");
            return -1;
        }

        /// <summary>
        /// Print config
        /// </summary>
        private void PrintConfig()
        {
            Trace.WriteLine("APPLICATION CONFIGURATION");

            foreach (int channelIndex in new int[] { txCi, rxCi })
            {
                Trace.WriteLine("-------------------------------------------------------------------");
                Trace.WriteLine("Configured Hardware Channel : " + driverConfig.channel[channelIndex].name);
                Trace.WriteLine("Hardware Driver Version     : " + driver.VersionToString(driverConfig.channel[channelIndex].driverVersion));
                Trace.WriteLine("Used Transceiver            : " + driverConfig.channel[channelIndex].transceiverName);
            }

            Trace.WriteLine("-------------------------------------------------------------------");
        }

    }
}
