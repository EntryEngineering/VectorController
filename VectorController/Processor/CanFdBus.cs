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

        private static uint canFdModeNoIso = 0;      // Global CAN FD ISO (default) / no ISO mode flag

        public CanFdBus(XLDriver xLDriver, XLDefine.XL_HardwareType xL_HardwareType) : base(xLDriver, xL_HardwareType, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN)
        {
            driver = xLDriver;
            hardwareType = xL_HardwareType;
        }


        public void TestCanFDBus() 
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
                Trace.WriteLine($"Channel compatible CanFD: {item.CanFdCompatible}");

                Trace.WriteLine("---------------------");
            }

            GetAppConfigAndSetAppConfig();



            RequestTheUserToAssignChannels();
            GetAccesMask();
            PrintAccessMask();
            OpenPort();

            //CheckPort();
            //ActivateChannel();
            //SetNotification();
            //ResetClock();

            ////RunRxThread();
            //for (int i = 0; i < 20; i++)
            //{
            //    CanTransmit();

            //}
        }


        //*******************************
        //**** Special CAN FD Bus API below
        //*******************************

        // xlCanFdSetConfiguration
        // xlCanTransmitEx
        // xlCanReceive
        // xlCanGetEventString

        internal void SetCanFdConfiguration() 
        {

        }



    }
}
