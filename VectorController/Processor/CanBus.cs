using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vxlapi_NET;

namespace VectorController.Processor
{
    internal class CanBus : CommonVector
    {

        public XLDriver driver { get; set; }

        public CanBus(XLDriver xLDriver) : base(xLDriver)
        {
            driver = xLDriver;
        }

        public void Test() 
        {
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

            CheckVCANCONF();

        }

        internal void CheckVCANCONF() 
        {
            // If the application name cannot be found in VCANCONF..
            if ((driver.XL_GetApplConfig(appName, 0, ref hwType, ref hwIndex, ref hwChannel, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN) != XLDefine.XL_Status.XL_SUCCESS) ||
                (driver.XL_GetApplConfig(appName, 1, ref hwType, ref hwIndex, ref hwChannel, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN) != XLDefine.XL_Status.XL_SUCCESS))
            {
                //...create the item with two CAN channels
                driver.XL_SetApplConfig(appName, 0, XLDefine.XL_HardwareType.XL_HWTYPE_NONE, 0, 0, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN);
                driver.XL_SetApplConfig(appName, 1, XLDefine.XL_HardwareType.XL_HWTYPE_NONE, 0, 0, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN);
                PrintAssignErrorAndPopupHwConf();
            }
        }





    }
}
