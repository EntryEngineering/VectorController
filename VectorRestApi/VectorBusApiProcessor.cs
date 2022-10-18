using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Timers;
using VectorBusLibrary.Processors;
using VectorRestApi.Model;
using vxlapi_NET;
using static vxlapi_NET.XLDefine;

namespace VectorRestApi
{
    public static class VectorBusApiProcessor
    {
        private static CanBus canBus;

        public static bool InitCanDone { get; set; } = false;

        /// <summary>
        /// Init Can Bus driver, driver config, open port, acvive channel, set notification and reset clock
        /// </summary>
        /// <returns></returns>
        public static XLDefine.XL_Status InitCanControloler()
        {
            XL_Status initStatus;
            canBus = new(XL_HardwareType.XL_HWTYPE_VN1610, "VectorCanBus_RestApi");

            Trace.WriteLine("****************************");
            Trace.WriteLine("CanBus - Vector");
            Trace.WriteLine("****************************");
            Trace.WriteLine("vxlapi_NET        : " + typeof(XLDriver).Assembly.GetName().Version);

            if (canBus.OpenDriver() != XL_Status.XL_SUCCESS || canBus.GetDriverConfig() != XL_Status.XL_SUCCESS)
            {
                return XL_Status.XL_ERROR;
            }

            canBus.GetAppConfigAndSetAppConfig();
            canBus.RequestTheUserToAssignChannels();

            CommonVector.GetAccesMask();
            Trace.WriteLine(CommonVector.PrintAccessMask());

            if (canBus.OpenPort() != XL_Status.XL_SUCCESS || canBus.ActivateChannel() != XL_Status.XL_SUCCESS || canBus.SetNotificationCanBus() != XL_Status.XL_SUCCESS || canBus.ResetClock() != XL_Status.XL_SUCCESS)
            {
                return XL_Status.XL_ERROR;
            }

            InitCanDone = true;
            return XL_Status.XL_SUCCESS;

        }


        private static MessageModel? tempMessage = null;
        private static bool? crcIncluded = null;

        public static MessageModel GetTestMessage()
        {

            List<SignalModel> signals = new();
            signals.Add(new SignalModel()
            {
                SignalName = "Crc",
                StartBit = 0,
                binaryLengh = 8,
                binaryData = new ushort[8] { 1, 1, 1, 1, 1, 1, 1, 1 }
            });

            signals.Add(new SignalModel()
            {
                SignalName = "Bz",
                StartBit = 8,
                binaryLengh = 4,
                binaryData = new ushort[4] { 1, 0, 1, 0 }
            });

            signals.Add(new SignalModel()
            {
                SignalName = "Data1",
                StartBit = 12,
                binaryLengh = 4,
                binaryData = new ushort[4] { 1, 1, 1, 0 }
            });

            signals.Add(new SignalModel()
            {
                SignalName = "Data2",
                StartBit = 16,
                binaryLengh = 1,
                binaryData = new ushort[1] { 1 }
            });

            MessageModel model = new()
            {
                MessageId = 0x3c0,
                DLC = 4,
                Signals = signals
            };



            string testMsgOut = JsonSerializer.Serialize(model, new JsonSerializerOptions() { WriteIndented = true });

            Trace.WriteLine(testMsgOut);

            return model;
        }


        public static void InitTxLoop()
        {
            txTimer.Elapsed += TimerForTx_Elapsed;
            txTimer.AutoReset = true;
            txTimer.Interval = 100;
            if (tempMessage == null)
            {
                tempMessage = GetTestMessage();
            }


        }

        public static void StartTxLoop()
        {

            Trace.WriteLine("sss");
            InitTxLoop();
            txTimer.Enabled = true;

        }

        public static void StopTxLoop()
        {
            if (txTimer.Enabled != false)
            {
                txTimer.Enabled = false;
            }
        }


        private static Timer txTimer = new Timer();
        private static void TimerForTx_Elapsed(object sender, ElapsedEventArgs e)
        {
            SendMessageEvent();
        }

        public static void SendCanMessageWithCrc(MessageModel message)
        {
            crcIncluded = true;
            txTimer.Enabled = true;
            MessageForSend(message);
            Trace.WriteLine("SetNewMessage - CRC");

        }  

        public static void SendCanMessageWithoutCrc(MessageModel message)
        {
            crcIncluded = false;
            txTimer.Enabled = true;
            //MessageForSend(message);
            MessageForSend(GetTestMessage());
            Trace.WriteLine("SetNewMessage - Non CRC");

        }


        private static XL_Status SendMessageEvent()
        {
            return canBus.CanTransmit(MessageForSend(tempMessage));
        }


        private static XLClass.xl_event_collection MessageForSend(MessageModel message) 
        {
            tempMessage = message;
            XLClass.xl_event_collection xlEventCollection = new XLClass.xl_event_collection(1);
            xlEventCollection.xlEvent[0].tagData.can_Msg.id = message.MessageId;
            xlEventCollection.xlEvent[0].tagData.can_Msg.dlc = message.DLC;

            if (crcIncluded == true)
            {

            }
            else
            {
                string competeleMsg = "";
                int count = 0;
                foreach (var item in message.Signals)
                {
                    competeleMsg += string.Join("-",item.binaryData[count]);
                    count += 1;

                }

                Trace.WriteLine($"competeleMsg: {competeleMsg}");
                xlEventCollection.xlEvent[0].tagData.can_Msg.data[0] = 15;
                
            }


            return xlEventCollection;
        }

    }
}
