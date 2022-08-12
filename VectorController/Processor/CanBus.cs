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

        public void TestCanBus() 
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

            GetAppConfigAndSetAppConfig();
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
        //*******************************
        //**** Special CAN Bus API below
        //*******************************


        // xlCanSetChannelMode
        // xlCanSetChannelOutput
        // xlCanSetReceiveMode
        // xlCanSetChannelTransceiver
        // xlCanSetChannelParams
        // xlCanSetChannelParamsC200
        // xlCanSetChannelBitrate
        // xlCanSetChannelAcceptance
        // xlCanAddAcceptanceRange
        // xlCanRemoveAcceptanceRange
        // xlCanResetAcceptance
        // xlCanRequestChipState - DONE
        // xlCanTransmit
        // xlCanFlushTransmitQueue


        /// <summary>
        /// Check port with function XL_CanRequestChipState
        /// </summary>
        /// <returns></returns>
        internal XLDefine.XL_Status CheckPort()
        {
            XLDefine.XL_Status status = xlDriver.XL_CanRequestChipState(portHandle, accessMask);
            Trace.WriteLine("Can Request Chip State: " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError("CheckPort");

            return status;
        }







    }
}
