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

        private static MessageModel? tempMessage = null;

        private static bool? crcIncluded = null;

        private static Timer txTimer = new Timer();

        private static int bzCounter = 0;

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

        public static MessageModel GetTestMessage()
        {

            //List<SignalModel> signals = new();
            //signals.Add(new SignalModel()
            //{
            //    SignalName = "Crc",
            //    StartBit = 0,
            //    binaryLengh = 8,
            //    binaryData = new ushort[8] { 1, 1, 1, 1, 1, 1, 1, 1 }
            //});

            //signals.Add(new SignalModel()
            //{
            //    SignalName = "Bz",
            //    StartBit = 8,
            //    binaryLengh = 4,
            //    binaryData = new ushort[4] { 1, 0, 1, 0 }
            //});

            //signals.Add(new SignalModel()
            //{
            //    SignalName = "Data1",
            //    StartBit = 12,
            //    binaryLengh = 4,
            //    binaryData = new ushort[4] { 1, 1, 1, 0 }
            //});

            //signals.Add(new SignalModel()
            //{
            //    SignalName = "Data2",
            //    StartBit = 16,
            //    binaryLengh = 1,
            //    binaryData = new ushort[1] { 1 }
            //});

            MessageModel model = new()
            {
                MessageId = 0x3c0,
                DLC = 4,
                Message = "10001101010010"
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
            InitTxLoop();
            txTimer.Enabled = true;
        }

        public static void StopTxLoop()
        {
            txTimer.Enabled = false;
        }

        public static void SetNewMessage(MessageModel newMessage)
        {
            txTimer.Enabled = false;
            tempMessage = newMessage;
            txTimer.Enabled = true;
        }

        // Loop for TX
        private static void TimerForTx_Elapsed(object sender, ElapsedEventArgs e)
        {
            // 1) couter Bz
            bzCounter += 1;


            // 2) Metoda pro výpočet CRC ze zprávy a Bz
            MessageModel message = GetFinalMessage(tempMessage, bzCounter);

            XLClass.xl_event_collection _Collection = ConvertMessageForSend(message);

            // 3) Metoda pro konečné odeslání zrpávy

            SendMessageEvent(_Collection);

            if (bzCounter >= 15)
            {
                bzCounter = 0;
            }
        }




        private static MessageModel GetFinalMessage(MessageModel originalMessage, int counter)
        {
            MessageModel editedMessage = new MessageModel();
            editedMessage.MessageId = originalMessage.MessageId;
            editedMessage.DLC = originalMessage.DLC;

            string addBzToOriginalMsg = $"{ConverterBinDecHex.DecimalToBinary(counter)}{originalMessage.Message}";
            Trace.WriteLine($"addBzToOriginalMsg: {addBzToOriginalMsg}");

            editedMessage.Message = $"{CrcProcessor.GetCrc(ConverterBinDecHex.BinaryToHex(addBzToOriginalMsg), 0xc3, CrcProcessor.Endianness.LittleEndian)}{addBzToOriginalMsg}";
            Trace.WriteLine($"editedMessage.Message: {editedMessage.Message}");

            return editedMessage;
        }


        private static XLClass.xl_event_collection ConvertMessageForSend(MessageModel message)
        {
            XLClass.xl_event_collection xlEventCollection = new XLClass.xl_event_collection(1);
            xlEventCollection.xlEvent[0].tagData.can_Msg.id = message.MessageId;
            xlEventCollection.xlEvent[0].tagData.can_Msg.dlc = message.DLC;



            return xlEventCollection;
        }

        private static XL_Status SendMessageEvent(XLClass.xl_event_collection rawMessage)
        {
            return canBus.CanTransmit(rawMessage);
        }

    }
}
