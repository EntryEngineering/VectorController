using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
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

        internal XLDriver xlDriver { get; set; }
        protected static String appName = "newVectorController";

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
        private static Thread rxThreadDDD;
        private static bool blockRxThread = false;
        // -----------------------------------------------------------------------------------------------


        // vše co mají společného Can,Can FD a Lin bus
        //XLDefine.XL_Status status;

        //**** Driver init
        //


        public CommonVector(XLDriver xLDriver, XLDefine.XL_HardwareType xL_HardwareType)
        {
            xlDriver = xLDriver;
            hwType = xL_HardwareType;
        }


        /// <summary>
        /// Open driver
        /// </summary>
        /// <returns>XLDefine.XL_Status</returns>
        public XLDefine.XL_Status OpenDriver()
        {
            XLDefine.XL_Status status = xlDriver.XL_OpenDriver();
            Trace.WriteLine("Open Driver       : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError("OpenDriver");

            return status;
        }

        /// <summary>
        /// Get driver config
        /// </summary>
        /// <returns>XLDefine.XL_Status</returns>
        public XLDefine.XL_Status GetDriverConfig()
        {
            XLDefine.XL_Status status = xlDriver.XL_GetDriverConfig(ref driverConfig);
            Trace.WriteLine("Get Driver Config : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError("GetDriverConfig");

            return status;
        }

        /// <summary>
        /// Get DLL verion of XL Driver
        /// </summary>
        /// <returns></returns>
        public string GetDLLVesrion()
        {
            return xlDriver.VersionToString(driverConfig.dllVersion);
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



        internal void CheckVCANCONF()
        {
            // If the application name cannot be found in VCANCONF..
            if ((xlDriver.XL_GetApplConfig(appName, 0, ref hwType, ref hwIndex, ref hwChannel, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN) != XLDefine.XL_Status.XL_SUCCESS) ||
                (xlDriver.XL_GetApplConfig(appName, 1, ref hwType, ref hwIndex, ref hwChannel, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN) != XLDefine.XL_Status.XL_SUCCESS))
            {
                //...create the item with two CAN channels
                xlDriver.XL_SetApplConfig(appName, 0, XLDefine.XL_HardwareType.XL_HWTYPE_NONE, 0, 0, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN);
                xlDriver.XL_SetApplConfig(appName, 1, XLDefine.XL_HardwareType.XL_HWTYPE_NONE, 0, 0, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN);
                PrintAssignErrorAndPopupHwConf();
            }
        }


        internal void GetAccesMask()
        {
            accessMask = txMask | rxMask;
            permissionMask = accessMask;
        }


        internal void PrintAccessMask()
        {
            Trace.WriteLine($"***************** TxMask:{txMask} - RxMask:{rxMask} - AcsMask:{accessMask} po accessMask = txMask | rxMask");
        }

        /// <summary>
        /// Open port
        /// </summary>
        /// <returns></returns>
        internal XLDefine.XL_Status OpenPort()
        {
            XLDefine.XL_Status status = xlDriver.XL_OpenPort(ref portHandle, appName, accessMask, ref permissionMask, 1024, XLDefine.XL_InterfaceVersion.XL_INTERFACE_VERSION, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN);
            Trace.WriteLine("Open Port             : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError("OpenPort");

            return status;
        }

        /// <summary>
        /// Check port
        /// </summary>
        /// <returns></returns>
        internal XLDefine.XL_Status CheckPort()
        {
            XLDefine.XL_Status status = xlDriver.XL_CanRequestChipState(portHandle, accessMask);
            Trace.WriteLine("Can Request Chip State: " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError("CheckPort");

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
            XLDefine.XL_Status status = xlDriver.XL_SetNotification(portHandle, ref tempInt, 1);
            xlEvWaitHandle.SafeWaitHandle = new SafeWaitHandle(new IntPtr(tempInt), true);

            Trace.WriteLine("Set Notification      : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError("SetNotification");

            return status;
        }

        /// <summary>
        /// Reset time stamp clock 
        /// </summary>
        /// <returns></returns>
        internal XLDefine.XL_Status ResetClock() 
        {
            XLDefine.XL_Status status = xlDriver.XL_ResetClock(portHandle);
            Trace.WriteLine("Reset Clock           : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError("ResetClock");

            return status;
        }

        /// <summary>
        /// Set time rate
        /// </summary>
        /// <param name="timeRate"></param>
        /// <returns></returns>
        internal XLDefine.XL_Status c(uint timeRate)
        {
            XLDefine.XL_Status status = xlDriver.XL_SetTimerRate(portHandle, timeRate);
            Trace.WriteLine("Time rate was set           : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError("DeactivateChannle");

            return status;
        }

        /// <summary>
        /// Deactivate channel
        /// </summary>
        /// <returns></returns>
        internal XLDefine.XL_Status DeactivateChannle() 
        {
            XLDefine.XL_Status status = xlDriver.XL_DeactivateChannel(portHandle, accessMask);
            Trace.WriteLine("Deactivate channel          : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError("DeactivateChannle");

            return status;
        }

        /// <summary>
        /// Close port
        /// </summary>
        /// <returns></returns>
        internal XLDefine.XL_Status ClosePort()
        {
            XLDefine.XL_Status status = xlDriver.XL_ClosePort(portHandle);
            Trace.WriteLine("Close port          : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError("ClosePort");

            return status;
        }

        /// <summary>
        /// Close driver
        /// </summary>
        /// <returns></returns>
        internal XLDefine.XL_Status CloseDriver()
        {
            XLDefine.XL_Status status = xlDriver.XL_CloseDriver();
            Trace.WriteLine("Close driver          : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError("CloseDriver");

            return status;
        }



        internal int PrintFunctionError(string functionName)
        {
            Trace.WriteLine($"ERROR: Function {functionName} call failed!");
            return -1;
        }

        private void PrintConfig()
        {
            Trace.WriteLine("APPLICATION CONFIGURATION");

            foreach (int channelIndex in new int[] { txCi, rxCi })
            {
                Trace.WriteLine("-------------------------------------------------------------------");
                Trace.WriteLine("Configured Hardware Channel : " + driverConfig.channel[channelIndex].name);
                Trace.WriteLine("Hardware Driver Version     : " + xlDriver.VersionToString(driverConfig.channel[channelIndex].driverVersion));
                Trace.WriteLine("Used Transceiver            : " + driverConfig.channel[channelIndex].transceiverName);
            }

            Trace.WriteLine("-------------------------------------------------------------------");
        }

        internal void PrintAssignErrorAndPopupHwConf()
        {
            Trace.WriteLine("Please check application settings of " + appName + " CAN1/CAN2,assign them to available hardware channels and press enter.");
            xlDriver.XL_PopupHwConfig();
        }
    }
}
