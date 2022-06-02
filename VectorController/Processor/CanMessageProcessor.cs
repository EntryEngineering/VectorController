﻿using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using vxlapi_NET;
using VectorController.Messages;

namespace VectorController.Processor
{
    internal class CanMessageProcessor
    {

        private static XLDriver CANDemo = new();
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
        private static bool blockRxThread = false;
        internal CancellationTokenSource _cancellationTokenSource = null;
        internal static string MessageId = "ALL";
        internal static List<string> msgIdList = new();
        internal static string dateTimeNowForFileName = DateTime.Now.ToString("CanBusLog yyyy_MM_DD HH-mm-ss");
        private static BaseCanMessage temporaryCanMessage = new();

        internal static TextBox messageTextBox = new();

        internal static Window testConsole = new()
        {
            Width = 800,
            Height = 200,
            Title = "CanBusConsole",
            Background = Brushes.DimGray,
        };



        internal CanMessageProcessor(XLDefine.XL_HardwareType xl_HardwareType, string aplicationName)
        {
            msgIdList.Add("ALL");
            hwType = xl_HardwareType;
            appName = aplicationName;
            Trace.WriteLine("constuctor");
            DriverInit();
            ChanelSetup();
        }

        private static BaseCanMessage GettempCanMessage()
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
            SaveMessageToFileByMessageId(value, messageId, dateTimeNowForFileName);

        }

        internal static void SaveMessageToFileByMessageId(BaseCanMessage message, string messageId, string fileName)
        {
            if (String.Equals(message.MessageId, messageId))
            {
                string path = $"{Environment.CurrentDirectory}\\message_{fileName}.csv";
                string outputString = $"{message.TimeStamp};{message.MessageId};{message.MessageValue};RAW_MSG[{message.RawCanMessage}]{Environment.NewLine}";
                Trace.Write(outputString);

                try
                {
                    if (!File.Exists(path))
                    {
                        File.AppendAllText(path, $"TimeStamp;MessageId;MessageValue;{Environment.NewLine}");
                    }
                    File.AppendAllText(path, outputString);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message.ToString());
                }
            }
            else if (String.Equals("ALL", messageId))
            {
                string path = $"{Environment.CurrentDirectory}\\message_{fileName}.csv";
                string outputString = $"{message.TimeStamp};{message.MessageId};{message.MessageValue};RAW_MSG[{message.RawCanMessage}]{Environment.NewLine}";
                Trace.Write(outputString);

                try
                {
                    if (!File.Exists(path))
                    {
                        File.AppendAllText(path, $"TimeStamp;MessageId;MessageValue;{Environment.NewLine}");
                    }
                    File.AppendAllText(path, outputString);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message.ToString());
                }
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

        internal static void DriverInit()
        {
            CANDemo.XL_OpenDriver();
            CANDemo.XL_GetDriverConfig(ref driverConfig);

            if ((CANDemo.XL_GetApplConfig(appName, 0, ref hwType, ref hwIndex, ref hwChannel, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN) != XLDefine.XL_Status.XL_SUCCESS) ||
                (CANDemo.XL_GetApplConfig(appName, 1, ref hwType, ref hwIndex, ref hwChannel, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN) != XLDefine.XL_Status.XL_SUCCESS))
            {
                CANDemo.XL_SetApplConfig(appName, 0, XLDefine.XL_HardwareType.XL_HWTYPE_NONE, 0, 0, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN);
                CANDemo.XL_SetApplConfig(appName, 1, XLDefine.XL_HardwareType.XL_HWTYPE_NONE, 0, 0, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN);
                CANDemo.XL_PopupHwConfig();
            }

            if (!GetAppChannelAndTestIsOk(0, ref txMask, ref txCi) || !GetAppChannelAndTestIsOk(1, ref rxMask, ref rxCi))
            {
                CANDemo.XL_PopupHwConfig();
            }

            Trace.WriteLine("DriverInit");
        }

        internal static void ChanelSetup()
        {
            accessMask = txMask | rxMask;
            permissionMask = accessMask;

            // Open port
            CANDemo.XL_OpenPort(ref portHandle, appName, accessMask, ref permissionMask, 1024, XLDefine.XL_InterfaceVersion.XL_INTERFACE_VERSION, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN);

            // Check port
            CANDemo.XL_CanRequestChipState(portHandle, accessMask);

            // Activate channel
            CANDemo.XL_ActivateChannel(portHandle, accessMask, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN, XLDefine.XL_AC_Flags.XL_ACTIVATE_NONE);

            // Initialize EventWaitHandle object with RX event handle provided by DLL
            int tempInt = -1;
            CANDemo.XL_SetNotification(portHandle, ref tempInt, 1);
            xlEvWaitHandle.SafeWaitHandle = new SafeWaitHandle(new IntPtr(tempInt), true);

            // Reset time stamp clock
            CANDemo.XL_ResetClock(portHandle);

        }

        internal void StartReceive()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            rxThread = new Thread(new ThreadStart(RXThread));
            rxThread.Start();
            Trace.WriteLine("rxThread.Start()");
        }

        internal void StopReceive()
        {
            //CancellationToken cancellationToken = new();

            //rxThread.Abort();
        }

        private static bool GetAppChannelAndTestIsOk(uint appChIdx, ref UInt64 chMask, ref int chIdx)
        {
            XLDefine.XL_Status status = CANDemo.XL_GetApplConfig(appName, appChIdx, ref hwType, ref hwIndex, ref hwChannel, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN);
            if (status != XLDefine.XL_Status.XL_SUCCESS)
            {
                Trace.WriteLine("XL_GetApplConfig      : " + status);
                Trace.WriteLine("\nERROR: Function call failed!\nPress any key to continue...");
            }

            chMask = CANDemo.XL_GetChannelMask(hwType, (int)hwIndex, (int)hwChannel);
            chIdx = CANDemo.XL_GetChannelIndex(hwType, (int)hwIndex, (int)hwChannel);
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
                        xlStatus = CANDemo.XL_Receive(portHandle, ref receivedEvent);

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
                                    SetTempCanMessage(ConvertMessage(CANDemo.XL_GetEventString(receivedEvent)), MessageId);
                                }
                            }
                        }
                    }
                }
                // No event occurred
            }
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

            //MessageValue
            string messageValueRaw = subStrings[5];
            baseCanMessage.MessageValue = messageValueRaw;

            //TID
            string tidRaw = subStrings[6];
            baseCanMessage.TID = tidRaw.Substring(tidRaw.IndexOf('=') + 1, tidRaw.Length - 4);

            return baseCanMessage;
        }
    }
}