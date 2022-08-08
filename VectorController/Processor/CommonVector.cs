using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using vxlapi_NET;

namespace VectorController.Processor
{
    internal class CommonVector
    {
        // -----------------------------------------------------------------------------------------------
        // Global variables
        // -----------------------------------------------------------------------------------------------
        // Driver access through XLDriver (wrapper)

        internal static XLDriver xlDriver { get; set; }
        //private static XLDriver CANDemo = new XLDriver();
        private static String appName = "newVectorController";

        // Driver configuration
        private static XLClass.xl_driver_config driverConfig = new XLClass.xl_driver_config();

        // Variables required by XLDriver
        private static XLDefine.XL_HardwareType hwType = XLDefine.XL_HardwareType.XL_HWTYPE_VN1610;
        private static uint hwIndex = 1;
        private static uint hwChannel = 1;
        private static int portHandle = -1;
        private static UInt64 accessMask = 0;
        private static UInt64 permissionMask = 0;
        private static UInt64 txMask = 0;
        private static UInt64 rxMask = 0;
        private static int txCi = -1;
        private static int rxCi = -1;
        private static EventWaitHandle xlEvWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, null);

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
        public List<string> GetListOfChannels() 
        {
            List<string> list = new List<string>();
            for (int i = 0; i < driverConfig.channelCount; i++)
            {
                list.Add($"{driverConfig.channel[i].name} - Channel mask: {driverConfig.channel[i].channelMask} - Transceiver name: {driverConfig.channel[i].transceiverName} - Serial number: {driverConfig.channel[i].serialNumber}");
            }
            return list;
        }

        // -GetAppConfig



        // -SetAppConfig
        // -GetChannelIndex
        // GetChannelMask


        // Openport







        private static int PrintFunctionError()
        {
            Trace.WriteLine("\nERROR: Function call failed!\nPress any key to continue...");
            return -1;
        }

        private static void PrintConfig()
        {
            Trace.WriteLine("\n\nAPPLICATION CONFIGURATION");

            foreach (int channelIndex in new int[] { txCi, rxCi })
            {
                Trace.WriteLine("-------------------------------------------------------------------");
                Trace.WriteLine("Configured Hardware Channel : " + driverConfig.channel[channelIndex].name);
                Trace.WriteLine("Hardware Driver Version     : " + xlDriver.VersionToString(driverConfig.channel[channelIndex].driverVersion));
                Trace.WriteLine("Used Transceiver            : " + driverConfig.channel[channelIndex].transceiverName);
            }

            Trace.WriteLine("-------------------------------------------------------------------\n");
        }

        private static void PrintAssignErrorAndPopupHwConf()
        {
            Trace.WriteLine("\nPlease check application settings of \"" + appName + " CAN1/CAN2\",\nassign them to available hardware channels and press enter.");
            xlDriver.XL_PopupHwConfig();
        }
    }
}
