using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using vxlapi_NET;
using VectorController.Models;

namespace VectorController.Processor
{
    internal partial class CommonVector
    {
        // -----------------------------------------------------------------------------------------------
        // Global variables
        // -----------------------------------------------------------------------------------------------
        // Driver access through XLDriver (wrapper)

        internal XLDriver xlDriver { get; set; }
        //private static XLDriver CANDemo = new XLDriver();
        protected static String appName = "newVectorController";

        // Driver configuration
        private static XLClass.xl_driver_config driverConfig = new XLClass.xl_driver_config();

        // Variables required by XLDriver
        protected static XLDefine.XL_HardwareType hwType = XLDefine.XL_HardwareType.XL_HWTYPE_VN1610;
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


        public CommonVector(XLDriver xLDriver)
        {
            xlDriver = xLDriver;
        }


        /// <summary>
        /// Open driver
        /// </summary>
        /// <returns>XLDefine.XL_Status</returns>
        public XLDefine.XL_Status OpenDriver()
        {
            XLDefine.XL_Status status = xlDriver.XL_OpenDriver();
            Trace.WriteLine("Open Driver       : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError();

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
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError();

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




        // -GetAppConfig



        // -SetAppConfig
        // -GetChannelIndex
        // GetChannelMask


        // Openport







        private int PrintFunctionError()
        {
            Trace.WriteLine("\nERROR: Function call failed!\nPress any key to continue...");
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
            Trace.WriteLine("\nPlease check application settings of \"" + appName + " CAN1/CAN2\",\nassign them to available hardware channels and press enter.");
            xlDriver.XL_PopupHwConfig();
        }
    }
}
