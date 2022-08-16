using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Timers;
using VectorController.Messages;
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
        internal static XLClass.xl_driver_config driverConfig = new XLClass.xl_driver_config();

        // Variables required by XLDriver
        protected static XLDefine.XL_HardwareType hwType;
        protected static uint hwIndex = 0;
        protected static uint hwChannel = 0;
        protected static int portHandle = -1;
        protected static UInt64 accessMask = 0;
        protected static UInt64 permissionMask = 0;
        protected static UInt64 txMask = 0;
        protected static UInt64 rxMask = 0;
        protected static int txCi = 0;
        protected static int rxCi = 0;
        protected static EventWaitHandle xlEvWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, null);

        // RX thread
        protected static Thread? rxThreadDDD = null;
        protected static bool blockRxThread = false;
        private XLDriver xLDriver;
        private XLDefine.XL_HardwareType xL_HardwareType;
        internal BaseCanMessage canMessage { get; set; } = new BaseCanMessage();

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
            XLDefine.XL_Status status = driver.XL_OpenPort(ref portHandle, appName, accessMask, ref permissionMask, 16000, XLDefine.XL_InterfaceVersion.XL_INTERFACE_VERSION_V4, commonBusType);
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
        internal XLDefine.XL_Status SetNotificationCanBus()
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
            //XLDefine.XL_Status status = driver.XL_ActivateChannel(portHandle, accessMask, commonBusType, XLDefine.XL_AC_Flags.XL_ACTIVATE_NONE);      // For Can
            XLDefine.XL_Status status = driver.XL_ActivateChannel(portHandle, accessMask, commonBusType, XLDefine.XL_AC_Flags.XL_ACTIVATE_RESET_CLOCK); // for CanFd
            Trace.WriteLine("Activate Channel      : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError("ActivateChannel");

            return status;
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
                VectorDeviceInfo info = new();
                info.ChannelName = driverConfig.channel[i].name;
                info.ChannelMask = driverConfig.channel[i].channelMask;
                info.TransceiverName = driverConfig.channel[i].transceiverName;
                info.SerialNumber = driverConfig.channel[i].serialNumber;


                if ((driverConfig.channel[i].channelCapabilities & XLDefine.XL_ChannelCapabilities.XL_CHANNEL_FLAG_CANFD_ISO_SUPPORT) == XLDefine.XL_ChannelCapabilities.XL_CHANNEL_FLAG_CANFD_ISO_SUPPORT)
                {
                    info.CanFdCompatible = true;
                }
                else
                {
                    info.CanFdCompatible = false;
                }

                list.Add(info);
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
        internal void PrintConfig()
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
