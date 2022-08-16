using vxlapi_NET;

namespace VectorController.Processor
{
    internal class LinBus : CommonVector
    {

        public XLDefine.XL_HardwareType hardwareType { get; set; }

        public LinBus(XLDriver xLDriver, XLDefine.XL_HardwareType xL_HardwareType) : base(xLDriver, xL_HardwareType, XLDefine.XL_BusTypes.XL_BUS_TYPE_LIN)
        {
            Driver = xLDriver;
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
