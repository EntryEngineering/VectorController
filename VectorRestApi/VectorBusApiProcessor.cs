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
            canBus = new(config.hardwareType, config.AppName);

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
            MessageModel model = new()
            {
                MessageId = 0x3c0,
                CycleTime = 100,
                DLC = 4,
                Message = "10101110101011101010"
            };
            crcIncluded = true;

            string testMsgOut = JsonSerializer.Serialize(model, new JsonSerializerOptions() { WriteIndented = true });
            return model;
        }

        public static void InitTxLoop()
        {
            txTimer.Elapsed += TimerForTx_Elapsed;
            txTimer.Interval = 100;
            txTimer.AutoReset = true;
           
            if (tempMessage == null)
            {
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

        public static void SetNewMessage(MessageModel newMessage,bool withCrcAndBz)
        {
            txTimer.Enabled = false;
            tempMessage = newMessage;
            crcIncluded = withCrcAndBz;
            txTimer.Enabled = true;
        }

        private static void TimerForTx_Elapsed(object sender, ElapsedEventArgs e)
        {
            long newRealInterval = 100/ tempMessage.CycleTime;
            for (long i = 0; i < newRealInterval; i++)
            {
                bzCounter += 1;
                MessageModel message;
                if (crcIncluded == true)
                {
                    message = GetFinalMessageWithCrc(tempMessage, bzCounter);
                    Trace.WriteLine("With CRC");
                }
                else
                {
                    message = GetFinalMessageWithoutCrc(tempMessage, bzCounter);
                    Trace.WriteLine("NON CRC");
                }

                if (bzCounter >= 15)
                {
                    bzCounter = 0;
                }
                XLClass.xl_event_collection _Collection = ConvertMessageForSend(message);
                SendMessageEvent(_Collection);
            }           
        }

        static int errCounter = 0;
        private static MessageModel GetFinalMessageWithCrc(MessageModel originalMessage, int counter)
        {
            //Trace.WriteLine($"Counter err : {errCounter}");
            MessageModel editedMessage = new MessageModel();
            editedMessage.MessageId = originalMessage.MessageId;
            editedMessage.DLC = originalMessage.DLC;
            string addBzToOriginalMsg = $"{ConverterBinDecHex.FillZerosToFull(ConverterBinDecHex.DecimalToBinary(counter))}{originalMessage.Message}";
            //Trace.WriteLine($"addBzToOriginalMsg: {addBzToOriginalMsg} and lengh: {addBzToOriginalMsg.Length}");
            var _tempResultCrc = "";

            try
            {
                string binaryToHex = ConverterBinDecHex.BinaryToHex(addBzToOriginalMsg);
                //Trace.WriteLine($"binaryToHex: {binaryToHex}");
                string crcResult = CrcProcessor.GetCrc(binaryToHex, 0xc3, CrcProcessor.Endianness.LittleEndian);
                //Trace.WriteLine($"crcResult HEX: {crcResult} and lengh: {crcResult.Length}");
                _tempResultCrc = ConverterBinDecHex.FillZerosToFull(ConverterBinDecHex.HexToBinary(crcResult));
                //Trace.WriteLine($"crcResult BINARY: {_tempResult} and length: {_tempResult.Length}");

            }
            catch (System.Exception ex)
            {
                Trace.WriteLine($"GetFinalMessage ERR: {ex.Message}");
                errCounter = errCounter + 1;
            }

            editedMessage.Message = $"{_tempResultCrc}{addBzToOriginalMsg}";
            return editedMessage;
        }

        private static MessageModel GetFinalMessageWithoutCrc(MessageModel originalMessage, int counter)
        {
            //Trace.WriteLine($"Counter err : {errCounter}");
            MessageModel editedMessage = new MessageModel();
            editedMessage.MessageId = originalMessage.MessageId;
            editedMessage.DLC = originalMessage.DLC;
            string addBzToOriginalMsg = $"{ConverterBinDecHex.FillZerosToFull(ConverterBinDecHex.DecimalToBinary(counter))}{originalMessage.Message}";
            //Trace.WriteLine($"addBzToOriginalMsg: {addBzToOriginalMsg} and lengh: {addBzToOriginalMsg.Length}");

            errCounter = errCounter + 1;
            editedMessage.Message = $"{addBzToOriginalMsg}";
            return editedMessage;
        }

        public static string CheckDlcAndBinaryLenghOfInsertingMessage(string messageBinary, int DLC, bool isThisMessageWithCrcAndBz)
        {
            const int crcAndBzLenghBits = 12;
            string _tempResult;
            int lengthOfMessage = messageBinary.Length;

            if (lengthOfMessage % 2 == 0)       // check if message length is even or odd
            {
                //is even
                if (isThisMessageWithCrcAndBz == true)
                {
                    int fullLengthMessageWithCrcAndBz = crcAndBzLenghBits + lengthOfMessage;
                    if ((fullLengthMessageWithCrcAndBz % 8 == 0) & (fullLengthMessageWithCrcAndBz / 8 == DLC))
                    {
                        return "OK";
                    }
                    else
                    {
                        return $"Error imput message: binary length of message is {lengthOfMessage} and DLC {DLC}-  not for whole BYTEs !";
                    }
                }
                else
                {
                    if ((lengthOfMessage % 8 == 0) & (lengthOfMessage / 8 == DLC))
                    {
                        return "OK";
                    }
                    else
                    {
                        return $"Error imput message: binary length of message is {lengthOfMessage} and DLC {DLC}-  not for whole BYTEs !";
                    }
                }
            }
            else
            {
                return $"Error imput message: binary length of message is odd (length: {lengthOfMessage} and DLC: {DLC})!";
            }
        }

        private static XLClass.xl_event_collection ConvertMessageForSend(MessageModel message)
        {
            XLClass.xl_event_collection xlEventCollection = new XLClass.xl_event_collection(1);
            xlEventCollection.xlEvent[0].tagData.can_Msg.id = message.MessageId;
            xlEventCollection.xlEvent[0].tagData.can_Msg.dlc = message.DLC;


            string __partOfMessageStringST = message.Message.Substring(0, 8);
            int _partOfMessageIntST = ConverterBinDecHex.BinaryToDecimal(__partOfMessageStringST);
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[0] = (byte)_partOfMessageIntST;


            for (int i = 1; i < message.DLC; i++)
            {
                int index = (i * 8) - 1;
                //Trace.WriteLine($"Msg before splitig: {message.Message} - Lengh: {message.Message.Length}");
                try
                {
                    //Trace.WriteLine($">>>start loop spliting....index:{index} message for split: {message.Message} Lengh: {message.Message.Length} and DLC: {message.DLC} and i: {i}");
                    string _partOfMessageString = message.Message.Substring(index, 8);
                    //Trace.WriteLine($"_partOfMessageString: {_partOfMessageString}");
                    int _partOfMessageInt = ConverterBinDecHex.BinaryToDecimal(_partOfMessageString);
                    //Trace.WriteLine($"_partOfMessageInt: {_partOfMessageInt}");
                    byte _tempByte = (byte)_partOfMessageInt;
                    //Trace.WriteLine($"_tempByte: {_tempByte} - index:{index}");
                    xlEventCollection.xlEvent[0].tagData.can_Msg.data[i] = _tempByte;
                    //Trace.WriteLine(">>>end loop spliting....");
                }
                catch (System.Exception ex)
                {
                    Trace.WriteLine($"***********************ConvertMessageForSend forLoop  err : {ex.Message}");
                }

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
