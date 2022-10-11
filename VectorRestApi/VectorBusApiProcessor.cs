using System;
using System.Diagnostics;
using System.Timers;
using VectorBusLibrary.Processors;
using VectorRestApi.Model;
using vxlapi_NET;
using static vxlapi_NET.XLDefine;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace VectorRestApi
{
    public static class VectorBusApiProcessor
    {
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




        private static BasicCanBusMessage tempMessage;

        private static BasicCanBusMessage GetTestMessgae()
        {
            BasicCanBusMessage _message = new BasicCanBusMessage();
            _message.MessageId = 0x3C0;
            _message.DLC = 4;
            _message.BZ = 0;
            _message.data[0] = "10101010"; // CRC
            _message.data[1] = "11110000"; // 4b. > BZ , 4b. Rst
            _message.data[2] = "10010011";  // 1b. 
            _message.data[3] = "00000000";


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
            txTimer.Enabled = true;
            tempMessage = message;
            Trace.WriteLine("SetNewMessage");

        }
         

        private static XL_Status SendMessageEvent(BasicCanBusMessage message)
        {
            XLClass.xl_event_collection xlEventCollection = new XLClass.xl_event_collection(1);
            xlEventCollection.xlEvent[0].tagData.can_Msg.id = message.MessageId;
            xlEventCollection.xlEvent[0].tagData.can_Msg.dlc = message.DLC;
            for (int i = 0; i < message.DLC; i++)
            {
                xlEventCollection.xlEvent[0].tagData.can_Msg.data[i] = (byte)ConverterBinDecHex.BinaryToDecimal(message.data[i]);
            }
            xlEventCollection.xlEvent[0].tag = XL_EventTags.XL_TRANSMIT_MSG;

            return canBus.CanTransmit(xlEventCollection);
        }



    }
}
