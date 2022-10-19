using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
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

        private static bool txRunning = false;

        private static Timer txTimer = new Timer();

        private static int bzCounter = 0;

        private static CanBusConfiguration busConfiguration = null;

        /// <summary>
        /// Init Can Bus driver, driver config, open port, acvive channel, set notification and reset clock
        /// </summary>
        /// <returns></returns>
        public static XLDefine.XL_Status InitCanControloler(CanBusConfiguration config)
        {
            busConfiguration = config;
            XL_Status initStatus;
            canBus = new(XL_HardwareType.XL_HWTYPE_VN1610, config.AppName);

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
            InitTxLoop();
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
                Message = "10101110101011101010"
            };



            string testMsgOut = JsonSerializer.Serialize(model, new JsonSerializerOptions() { WriteIndented = true });

            Trace.WriteLine(testMsgOut);

            return model;
        }

        public static void InitTxLoop()
        {
            txTimer.Elapsed += TimerForTx_Elapsed;
            txTimer.AutoReset = true;
            txTimer.Interval = busConfiguration.CycleTime;
            if (tempMessage == null)
            {
                Trace.WriteLine("InitTxLoop > set default msg");
                tempMessage = GetTestMessage();
            }
        }

        public static void StartTxLoop()
        {
            txTimer.Enabled = true;
            txRunning = true;
        }

        public static void StopTxLoop()
        {
            txRunning = false;
            txTimer.Enabled = false;
        }

        public static void SetNewMessage(MessageModel newMessage)
        {
            txTimer.Enabled = false;
            tempMessage = newMessage;
            txTimer.Enabled = true;
        }


        private static void TimerForTx_Elapsed(object sender, ElapsedEventArgs e)
        {


            bzCounter += 1;
            Trace.WriteLine($"bzCounter: {bzCounter}");

            MessageModel message = GetFinalMessage(tempMessage, bzCounter);

            if (bzCounter >= 15)
            {
                Trace.WriteLine($"bzCounter RESET: {bzCounter}");
                bzCounter = 0;
            }

            Trace.WriteLine($"MSG: {message.Message} -- Lengh: {message.Message.Length}");

            XLClass.xl_event_collection _Collection = ConvertMessageForSend(message);

            SendMessageEvent(_Collection);


        }




        private static MessageModel GetFinalMessage(MessageModel originalMessage, int counter)
        {
            MessageModel editedMessage = new MessageModel();
            editedMessage.MessageId = originalMessage.MessageId;
            editedMessage.DLC = originalMessage.DLC;

            string addBzToOriginalMsg = $"{ConvertDecToBin4bit(counter)}{originalMessage.Message}";
            Trace.WriteLine($"addBzToOriginalMsg: {addBzToOriginalMsg}");
            var _tempCrc = "";


            try
            {
                _tempCrc = ConverterBinDecHex.HexToBinary(CrcProcessor.GetCrc(ConverterBinDecHex.BinaryToHex(addBzToOriginalMsg), 0xc3, CrcProcessor.Endianness.LittleEndian));
            }
            catch (System.Exception ex)
            {

                Trace.WriteLine($"GetFinalMessage ERR: {ex.Message}");
            }


            editedMessage.Message = $"{_tempCrc}{addBzToOriginalMsg}";
            Trace.WriteLine($"editedMessage.Message: {editedMessage.Message}");

            return editedMessage;
        }

        private static string ConvertDecToBin4bit(int decimalNumber)
        {
            string _tempBin = ConverterBinDecHex.DecimalToBinary(decimalNumber);
            int numberOfZeroToFill = 4 - _tempBin.Length;

            if (numberOfZeroToFill > 0)
            {
                if (numberOfZeroToFill == 3)
                {
                    return $"000{_tempBin}";
                }
                else if (numberOfZeroToFill == 2)
                {
                    return $"00{_tempBin}";
                }
                else if (numberOfZeroToFill == 1)
                {
                    return $"0{_tempBin}";
                }
                else
                {
                    return _tempBin;
                }
            }
            else
            {
                return _tempBin;
            }


        }


        private static XLClass.xl_event_collection ConvertMessageForSend(MessageModel message)
        {
            XLClass.xl_event_collection xlEventCollection = new XLClass.xl_event_collection(1);
            xlEventCollection.xlEvent[0].tagData.can_Msg.id = message.MessageId;
            xlEventCollection.xlEvent[0].tagData.can_Msg.dlc = message.DLC;


            xlEventCollection.xlEvent[0].tagData.can_Msg.data[0] = (byte)ConverterBinDecHex.BinaryToDecimal(message.Message.Substring(0, 8));
            for (int i = 1; i < message.DLC; i++)
            {
                int index = (i*8)-1;
                xlEventCollection.xlEvent[0].tagData.can_Msg.data[i] = (byte)ConverterBinDecHex.BinaryToDecimal(message.Message.Substring(index, 8));
            }

            xlEventCollection.xlEvent[0].tag = XL_EventTags.XL_TRANSMIT_MSG;
            return xlEventCollection;
        }

        private static XL_Status SendMessageEvent(XLClass.xl_event_collection rawMessage)
        {
            return canBus.CanTransmit(rawMessage);
        }

    }
}
