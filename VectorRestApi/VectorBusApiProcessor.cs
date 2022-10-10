using System;
using System.Diagnostics;
using System.Timers;
using VectorBusLibrary.Processors;
using VectorRestApi.Model;
using vxlapi_NET;
using static vxlapi_NET.XLDefine;
using VectorBusLibrary.Processors;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace VectorRestApi
{
    public static class VectorBusApiProcessor
    {
        //-------------------------------------------------------------

        private static BasicCanBusMessage tempMessage;

        private static BasicCanBusMessage GetTestMessgae()
        {
            BasicCanBusMessage _message = new BasicCanBusMessage();
            _message.MessageId = 0x3C0;
            _message.DLC = 4;
            _message.data0Byte = "10101010";
            _message.data1Byte = "11110000";
            _message.data2Byte = "10010011";
            _message.data3Byte = "00000000";


            return _message;
        }


        public static void InitTxLoop()
        {
            txTimer.Elapsed += TimerForTx_Elapsed;
            txTimer.AutoReset = true;
            txTimer.Interval = 100;
            if (tempMessage == null)
            {
                tempMessage = GetTestMessgae();
            }
            

        }

        public static void StartTxLoop()
        {
            InitTxLoop();
            txTimer.Enabled = true;
        }

        public static void StopTxLoop()
        {
            txTimer.Enabled = false;
        }


        private static Timer txTimer = new Timer();
        private static void TimerForTx_Elapsed(object sender, ElapsedEventArgs e)
        {
            SendMessageEvent(tempMessage);
        }

        public static void SetNewMessage(BasicCanBusMessage message) 
        {
            InitTxLoop();
            txTimer.Enabled = true;
            tempMessage = message;
            Trace.WriteLine("SetNewMessage");

        }
         

        private static XL_Status SendMessageEvent(BasicCanBusMessage message)
        {
            XLClass.xl_event_collection xlEventCollection = new XLClass.xl_event_collection(1);
            xlEventCollection.xlEvent[0].tagData.can_Msg.id = message.MessageId;
            xlEventCollection.xlEvent[0].tagData.can_Msg.dlc = message.DLC;
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[0] = (byte)ConverterBinDecHex.BinaryToDecimal(message.data0Byte);
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[1] = (byte)ConverterBinDecHex.BinaryToDecimal(message.data1Byte);
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[2] = (byte)ConverterBinDecHex.BinaryToDecimal(message.data2Byte);
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[3] = (byte)ConverterBinDecHex.BinaryToDecimal(message.data3Byte);



            xlEventCollection.xlEvent[0].tag = XL_EventTags.XL_TRANSMIT_MSG;

            return canBus.CanTransmit(xlEventCollection);
        }


        private static CanBus canBus;

        public static void InitCanControloler()
        {
            canBus = new(XL_HardwareType.XL_HWTYPE_VN1610, "VectorCanBus_RestApi");
            Trace.WriteLine("****************************");
            Trace.WriteLine("CanBus - Vector");
            Trace.WriteLine("****************************");

            Trace.WriteLine("vxlapi_NET        : " + typeof(XLDriver).Assembly.GetName().Version);
            canBus.OpenDriver();
            canBus.GetDriverConfig();
            canBus.GetAppConfigAndSetAppConfig();
            canBus.RequestTheUserToAssignChannels();
            CommonVector.GetAccesMask();
            Trace.WriteLine(CommonVector.PrintAccessMask());
            canBus.OpenPort();
            canBus.ActivateChannel();
            canBus.SetNotificationCanBus();
            canBus.ResetClock();
        }
    }
}
