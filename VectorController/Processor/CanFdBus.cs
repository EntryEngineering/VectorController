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
    internal class CanFdBus : CommonVector
    {

        public XLDriver driver { get; set; }
        public XLDefine.XL_HardwareType hardwareType { get; set; }

        public CanFdBus(XLDriver xLDriver, XLDefine.XL_HardwareType xL_HardwareType) : base(xLDriver, xL_HardwareType, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN)
        {
            driver = xLDriver;
            hardwareType = xL_HardwareType;
        }


        public void TestCanFDBus() 
        {

        }


        //*******************************
        //**** Special CAN FD Bus API below
        //*******************************

        // xlCanFdSetConfiguration
        // xlCanTransmitEx
        // xlCanReceive
        // xlCanGetEventString





    }
}
