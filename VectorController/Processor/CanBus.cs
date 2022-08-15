using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using vxlapi_NET;
using static vxlapi_NET.XLDefine;

namespace VectorController.Processor
{
    internal class CanBus : CommonVector
    {

        public XLDriver driver { get; set; }
        public XL_HardwareType hardwareType { get; set; }


        public CanBus(XLDriver xLDriver, XL_HardwareType xL_HardwareType) : base(xLDriver, xL_HardwareType, XL_BusTypes.XL_BUS_TYPE_CAN)
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
            ActivateChannel();
            SetNotification();
            ResetClock();

            //RunRxThread();
            for (int i = 0; i < 20; i++)
            {
                CanTransmit();

            }
            


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
        // xlCanTransmit - DONE
        // xlCanFlushTransmitQueue


        /// <summary>
        /// Check port with function XL_CanRequestChipState
        /// </summary>
        /// <returns></returns>
        internal XLDefine.XL_Status CheckPort()
        {
            XLDefine.XL_Status status = base.driver.XL_CanRequestChipState(portHandle, accessMask);
            Trace.WriteLine("Can Request Chip State: " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError("CheckPort");

            return status;
        }


        /// <summary>
        /// Transmit Can Bus message
        /// </summary>
        internal void CanTransmit()
        {
            XLDefine.XL_Status txStatus;
            XLClass.xl_event_collection xlEventCollection = new XLClass.xl_event_collection(1);

            // event 1
            xlEventCollection.xlEvent[0].tagData.can_Msg.id = 0x3C0;
            xlEventCollection.xlEvent[0].tagData.can_Msg.dlc = 4;
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[0] = 1;
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[1] = 2;
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[2] = 3;
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[3] = 4;
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[4] = 5;
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[5] = 6;
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[6] = 7;
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[7] = 8;
            xlEventCollection.xlEvent[0].tag = XLDefine.XL_EventTags.XL_TRANSMIT_MSG;

            txStatus = base.driver.XL_CanTransmit(portHandle, txMask, xlEventCollection);
            Trace.WriteLine("Transmit Message      : " + txStatus);
        }


    }
}
