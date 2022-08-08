using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using VectorController.Messages;
using vxlapi_NET;

namespace VectorController.Processor
{
    internal class CanMessageProcessor
    {
        private static XLDriver canBusDriver = new();
        private static string appName;
        private static XLClass.xl_driver_config driverConfig = new();
        private static XLDefine.XL_HardwareType hwType;
        private static uint hwIndex = 1;
        private static uint hwChannel = 1;
        private static int portHandle = -1;
        private static ulong accessMask = 0;
        private static ulong permissionMask = 0;
        private static ulong txMask = 0;
        private static ulong rxMask = 0;
        private static int txCi = -1;
        private static int rxCi = -1;
        private static EventWaitHandle xlEvWaitHandle = new(false, EventResetMode.AutoReset, null);
        private static Thread rxThread;
        private static Thread txThread;
        private static bool blockRxThread = false;
        internal CancellationTokenSource _cancellationTokenSource = null;
        internal static string MessageId = "ALL";
        internal static List<string> msgIdList = new();
        internal static string internalTimeStamp = DateTime.Now.ToString("yyyy_MM_DD HH-mm-ss");


        private static BaseCanMessage temporaryCanMessage = new();

        internal CanMessageProcessor(XLDefine.XL_HardwareType xl_HardwareType, string aplicationName)
        {
            msgIdList.Add("ALL");

            hwType = xl_HardwareType;
            appName = aplicationName;

            Trace.WriteLine($"--- Application {aplicationName} connected with {xl_HardwareType}---");
            DriverInit(aplicationName, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN);
            ChanelSetup();
        }


        public BaseCanMessage GettempCanMessage()
        {
            return temporaryCanMessage;
        }

        private static void SetTempCanMessage(BaseCanMessage value, string messageId)
        {
            temporaryCanMessage = value;

            if (!msgIdList.Contains(temporaryCanMessage.MessageId))
            {
                msgIdList.Add(temporaryCanMessage.MessageId);
            }
            SaveMessageToFileByMessageId(value, messageId, "CanBus" + internalTimeStamp);
            SaveMesageWhenDataChanged(value, messageId, "CanBus" + internalTimeStamp);



        }

        internal static void PrintMessage(BaseCanMessage message)
        {

            Trace.WriteLine($"TimeStamp:{message.TimeStamp} MessageId:{message.MessageId.PadLeft(10)} MessageValueHex:{message.MessageValueHex.PadLeft(15)} MessageValueBinary:{message.MessageValueBinary} TestKlema: {PartOfMessage(message.MessageValueBinary, 22, 2)}");
        }

        internal static void SaveMessageToFileByMessageId(BaseCanMessage message, string messageId, string fileName)
        {
            if (String.Equals(message.MessageId, messageId))
            {
                string path = $"{Environment.CurrentDirectory}\\message_{fileName}.csv";
                string outputString = $"{message.TimeStamp};{message.MessageId};{message.MessageValueHex};{Environment.NewLine}";
                PrintMessage(message);

                try
                {
                    if (!File.Exists(path))
                    {
                        File.AppendAllText(path, $"TimeStamp;MessageId;MessageValue;{Environment.NewLine}", System.Text.Encoding.ASCII);
                    }
                    File.AppendAllText(path, outputString, System.Text.Encoding.ASCII);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message.ToString());
                }
            }
            else if (String.Equals("ALL", messageId))
            {
                string path = $"{Environment.CurrentDirectory}\\message_{fileName}.csv";
                string outputString = $"{message.TimeStamp};{message.MessageId};{message.MessageValueHex};{Environment.NewLine}";
                PrintMessage(message);

                try
                {
                    if (!File.Exists(path))
                    {
                        File.AppendAllText(path, $"TimeStamp;MessageId;MessageValue;{Environment.NewLine}", System.Text.Encoding.ASCII);
                    }
                    File.AppendAllText(path, outputString, System.Text.Encoding.ASCII);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message.ToString());
                }
            }
        }

        internal static void SaveMesageWhenDataChanged(BaseCanMessage message, string messageId, string fileName)
        {
            string msgDataTemp = "";
            string partOfMessage = PartOfMessage(message.MessageValueBinary, 22, 2);

            if (String.Equals(message.MessageId, messageId) && !String.Equals(msgDataTemp, partOfMessage))
            {
                string path = $"{Environment.CurrentDirectory}\\changesOnId{messageId}_{fileName}.csv";
                string outputString = $"{internalTimeStamp};{message.MessageId};{partOfMessage};{Environment.NewLine}";
                PrintMessage(message);

                try
                {
                    if (!File.Exists(path))
                    {
                        File.AppendAllText(path, $"TimeStamp;MessageId;MessageValue;{Environment.NewLine}", System.Text.Encoding.ASCII);
                        msgDataTemp = partOfMessage;
                    }
                    File.AppendAllText(path, outputString, System.Text.Encoding.ASCII);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message.ToString());
                }
            }
            else
            {
                msgDataTemp = partOfMessage;
            }
        }

        internal void SetMessageIdFilter(string messageId)
        {
            MessageId = messageId;
        }
        internal List<string> GetListOfMsgId()
        {
            msgIdList.Sort();
            return msgIdList;
        }

        internal static void DriverInit(string aplicationName, XLDefine.XL_BusTypes xL_BusTypes)
        {
            canBusDriver.XL_OpenDriver();
            canBusDriver.XL_GetDriverConfig(ref driverConfig);

            if ((canBusDriver.XL_GetApplConfig(aplicationName, 0, ref hwType, ref hwIndex, ref hwChannel, xL_BusTypes) != XLDefine.XL_Status.XL_SUCCESS) ||
                (canBusDriver.XL_GetApplConfig(aplicationName, 1, ref hwType, ref hwIndex, ref hwChannel, xL_BusTypes) != XLDefine.XL_Status.XL_SUCCESS))
            {
                canBusDriver.XL_SetApplConfig(aplicationName, 0, XLDefine.XL_HardwareType.XL_HWTYPE_NONE, 0, 0, xL_BusTypes);
                canBusDriver.XL_SetApplConfig(aplicationName, 1, XLDefine.XL_HardwareType.XL_HWTYPE_NONE, 0, 0, xL_BusTypes);
                canBusDriver.XL_PopupHwConfig();
            }

            if (!GetAppChannelAndTestIsOk(0, ref txMask, ref txCi) || !GetAppChannelAndTestIsOk(1, ref rxMask, ref rxCi))
            {
                canBusDriver.XL_PopupHwConfig();
            }

            Trace.WriteLine("APP_STATE: DriverInit");
        }

        internal static void ChanelSetup()
        {
            Trace.WriteLine($"txMask: {txMask} and rxMask:{rxMask}");

            accessMask = txMask | rxMask;
            permissionMask = accessMask;

            Trace.WriteLine($"accessMask: {accessMask}");
            Trace.WriteLine($"permissionMask: {permissionMask}");
            // Open port
            canBusDriver.XL_OpenPort(ref portHandle, appName, accessMask, ref permissionMask, 1024, XLDefine.XL_InterfaceVersion.XL_INTERFACE_VERSION, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN);

            // Check port
            canBusDriver.XL_CanRequestChipState(portHandle, accessMask);

            // Activate channel
            canBusDriver.XL_ActivateChannel(portHandle, accessMask, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN, XLDefine.XL_AC_Flags.XL_ACTIVATE_NONE);

            // Initialize EventWaitHandle object with RX event handle provided by DLL
            int tempInt = -1;
            canBusDriver.XL_SetNotification(portHandle, ref tempInt, 1);
            xlEvWaitHandle.SafeWaitHandle = new SafeWaitHandle(new IntPtr(tempInt), true);

            // Reset time stamp clock
            canBusDriver.XL_ResetClock(portHandle);

            Trace.WriteLine("APP_STATE: ChanelSetup");
        }

        internal void StartReceive()
        {
            rxThread = new Thread(new ThreadStart(RXThread));
            rxThread.Start();

            Trace.WriteLine("APP_STATE: StartReceive");
        }

        internal void StopReceive()
        {
            Trace.WriteLine("APP_STATE: STOPtReceive");
            canBusDriver.XL_ClosePort(portHandle);
            canBusDriver.XL_CloseDriver();
        }

        internal void StartTransmit()
        {
            txThread = new Thread(new ThreadStart(TXThread));
            txThread.Start();
        }

        private static bool GetAppChannelAndTestIsOk(uint appChIdx, ref UInt64 chMask, ref int chIdx)
        {
            XLDefine.XL_Status status = canBusDriver.XL_GetApplConfig(appName, appChIdx, ref hwType, ref hwIndex, ref hwChannel, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN);
            if (status != XLDefine.XL_Status.XL_SUCCESS)
            {
                Trace.WriteLine("XL_GetApplConfig      : " + status);
                Trace.WriteLine("\nERROR: Function call failed!\nPress any key to continue...");
            }

            chMask = canBusDriver.XL_GetChannelMask(hwType, (int)hwIndex, (int)hwChannel);
            chIdx = canBusDriver.XL_GetChannelIndex(hwType, (int)hwIndex, (int)hwChannel);
            if (chIdx < 0 || chIdx >= driverConfig.channelCount)
            {
                // the (hwType, hwIndex, hwChannel) triplet stored in the application configuration does not refer to any available channel.
                return false;
            }

            // test if CAN is available on this channel
            return (driverConfig.channel[chIdx].channelBusCapabilities & XLDefine.XL_BusCapabilities.XL_BUS_ACTIVE_CAP_CAN) != 0;
        }

        internal static void RXThread()
        {
            // Create new object containing received data 
            XLClass.xl_event receivedEvent = new();

            // Result of XL Driver function calls
            XLDefine.XL_Status xlStatus = XLDefine.XL_Status.XL_SUCCESS;

            // Note: this thread will be destroyed by MAIN
            while (true)
            {
                // Wait for hardware events
                if (xlEvWaitHandle.WaitOne(1000))
                {
                    // ...init xlStatus first
                    xlStatus = XLDefine.XL_Status.XL_SUCCESS;

                    // afterwards: while hw queue is not empty...
                    while (xlStatus != XLDefine.XL_Status.XL_ERR_QUEUE_IS_EMPTY)
                    {
                        // ...block RX thread to generate RX-Queue overflows
                        while (blockRxThread) { Thread.Sleep(1000); }

                        // ...receive data from hardware.
                        xlStatus = canBusDriver.XL_Receive(portHandle, ref receivedEvent);

                        //  If receiving succeed....
                        if (xlStatus == XLDefine.XL_Status.XL_SUCCESS)
                        {
                            if ((receivedEvent.flags & XLDefine.XL_MessageFlags.XL_EVENT_FLAG_OVERRUN) != 0)
                            {
                                Console.WriteLine("-- XL_EVENT_FLAG_OVERRUN --");
                            }

                            // ...and data is a Rx msg...
                            if (receivedEvent.tag == XLDefine.XL_EventTags.XL_RECEIVE_MSG)
                            {
                                if ((receivedEvent.tagData.can_Msg.flags & XLDefine.XL_MessageFlags.XL_CAN_MSG_FLAG_OVERRUN) != 0)
                                {
                                    Console.WriteLine("-- XL_CAN_MSG_FLAG_OVERRUN --");
                                }

                                // ...check various flags
                                if ((receivedEvent.tagData.can_Msg.flags & XLDefine.XL_MessageFlags.XL_CAN_MSG_FLAG_ERROR_FRAME)
                                    == XLDefine.XL_MessageFlags.XL_CAN_MSG_FLAG_ERROR_FRAME)
                                {
                                    Console.WriteLine("ERROR FRAME");
                                }

                                else if ((receivedEvent.tagData.can_Msg.flags & XLDefine.XL_MessageFlags.XL_CAN_MSG_FLAG_REMOTE_FRAME)
                                    == XLDefine.XL_MessageFlags.XL_CAN_MSG_FLAG_REMOTE_FRAME)
                                {
                                    Console.WriteLine("REMOTE FRAME");
                                }

                                else
                                {
                                    SetTempCanMessage(ConvertMessage(canBusDriver.XL_GetEventString(receivedEvent)), MessageId);
                                }
                            }
                        }
                    }
                }
                // No event occurred
            }
        }

        internal void TXThread()
        {
            while (true)
            {
                Thread.Sleep(2000);
                Trace.WriteLine("TX send");
            }

        }


        internal void TXProcess(uint msgId, byte msgDlc, byte bytePos0, byte bytePos1, byte bytePos2, byte bytePos3, byte bytePos4, byte bytePos5, byte bytePos6, byte bytePos7)
        {
            //blockRxThread = rxThread.IsAlive;
            //rxThread.Abort();



            XLDefine.XL_Status txStatus;
            XLClass.xl_event_collection xlEventCollection = new XLClass.xl_event_collection(1);
            xlEventCollection.xlEvent[0].tagData.can_Msg.id = msgId;
            xlEventCollection.xlEvent[0].tagData.can_Msg.dlc = msgDlc;
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[0] = bytePos0;
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[1] = bytePos1;
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[2] = bytePos2;
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[3] = bytePos3;
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[4] = bytePos4;
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[5] = bytePos5;
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[6] = bytePos6;
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[7] = bytePos7;
            xlEventCollection.xlEvent[0].tag = XLDefine.XL_EventTags.XL_TRANSMIT_MSG;
            txStatus = canBusDriver.XL_CanTransmit(portHandle, txMask, xlEventCollection);



        }

        internal static BaseCanMessage ConvertMessage(string input)
        {
            string[] subStrings = input.Split(' ');
            BaseCanMessage baseCanMessage = new();

            baseCanMessage.RawCanMessage = input;

            //Channel number
            string channelNumberRaw = subStrings[1];
            baseCanMessage.ChannelNumber = channelNumberRaw.Substring(channelNumberRaw.IndexOf('=') + 1, channelNumberRaw.Length - 3);

            //TimeStamp
            string timeStanpRaw = subStrings[2];
            baseCanMessage.TimeStamp = timeStanpRaw.Substring(timeStanpRaw.IndexOf('=') + 1, timeStanpRaw.Length - 3);

            //MessageId
            string messageIdRaw = subStrings[3];
            baseCanMessage.MessageId = messageIdRaw.Substring(messageIdRaw.IndexOf('=') + 1, messageIdRaw.Length - 3);

            //MessageLenght
            string messageLenghtRaw = subStrings[4];
            baseCanMessage.DLC = messageLenghtRaw.Substring(messageLenghtRaw.IndexOf('=') + 1, messageLenghtRaw.Length - 3);


            string messageValueRaw = subStrings[5];
            //MessageValue - HEX
            baseCanMessage.MessageValueHex = messageValueRaw;
            //MessageValue - BINARY
            baseCanMessage.MessageValueBinary = HexToBinary(messageValueRaw);





            ////TID 
            //string tidRaw = subStrings[6];
            //baseCanMessage.TID = tidRaw.Substring(tidRaw.IndexOf('=') + 1, tidRaw.Length - 4);

            ////TID
            string tidRaw = subStrings[6];
            baseCanMessage.TID = tidRaw.Substring(tidRaw.IndexOf('=') + 1, tidRaw.Length - 4);
            if (String.Equals(tidRaw, "TX"))
            {
                tidRaw = subStrings[7];
                baseCanMessage.TID = tidRaw.Substring(tidRaw.IndexOf('=') + 1, tidRaw.Length - 4);
            }
            else
            {
                baseCanMessage.TID = tidRaw.Substring(tidRaw.IndexOf('=') + 1, tidRaw.Length - 4);
            }

            return baseCanMessage;
        }

        internal static string HexToBinary(string input)
        {
            return string.Join(string.Empty, input.Select(c => Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(4, '0')));
        }

        internal static string PartOfMessage(string binaryMessage, int startBit, int lenght)
        {
            if (binaryMessage.Length >= startBit + lenght)
            {
                return binaryMessage.Substring(startBit, lenght);
            }
            else
            {
                return "err";
            }
        }


        internal string SwapEndianMotorlaToIntel(string input) //  Big to Little
        {

            if (!String.IsNullOrEmpty(input))
            {
                // String to int32

                Int32 intDatatemp = Convert.ToInt32(input);

                // Int32 to byte array

                byte[] bytesTemp = BitConverter.GetBytes(intDatatemp);

                // Check if input is Big - array of bytes reverse

                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(bytesTemp);
                }

                // array bytes to string (int34)


                return System.Text.Encoding.Default.GetString(bytesTemp);
            }
            else
            {
                throw new("string is null or empty");
            }
        }

    }
}
