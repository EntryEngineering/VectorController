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
    internal class LinBus : CommonVector
    {

        public XLDriver driver { get; set; }
        public XLDefine.XL_HardwareType hardwareType { get; set; }

        public LinBus(XLDriver xLDriver, XLDefine.XL_HardwareType xL_HardwareType) : base(xLDriver, xL_HardwareType)
        {
            driver = xLDriver;
            hardwareType = xL_HardwareType;
        }


        public void TestLinBus()
        {

        }



        //*******************************
        //**** Special LIN Bus API below
        //*******************************



        // xlLinSetChannelParams
        // xlLinSetDLC
        // xlLinSetChecksum
        // xlLinSetSlave
        // xlLinSwitchSlave
        // xlLinSendRequest
        // xlLinWakeUp
        // xlLinSetSleepMode



    }
}
