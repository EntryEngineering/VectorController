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
    internal class CanBus : CommonVector
    {

        public XLDriver driver { get; set; }
        public XLDefine.XL_HardwareType hardwareType { get; set; }


        public CanBus(XLDriver xLDriver, XLDefine.XL_HardwareType xL_HardwareType) : base(xLDriver, xL_HardwareType)
        {
            driver = xLDriver;
            hardwareType = xL_HardwareType;
        }

        public void Test() 
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

                Trace.WriteLine("---------------------");
            }

            CheckVCANCONF();
            RequestTheUserToAssignChannels();
            GetAccesMask();
            PrintAccessMask();
            OpenPort();
            CheckPort();
            SetNotification();
            ResetClock();
            ActivateChannel();
            RunRxThread();


        }


        internal XLDefine.XL_Status ActivateChannel() 
        {
            XLDefine.XL_Status status = driver.XL_ActivateChannel(portHandle, accessMask, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN, XLDefine.XL_AC_Flags.XL_ACTIVATE_NONE);
            Trace.WriteLine("Activate Channel      : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError("ActivateChannel");

            return status;
        }


    }
}
