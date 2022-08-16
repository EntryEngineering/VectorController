using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using vxlapi_NET;

namespace VectorController.Processor
{

    internal class BaseCanMessage
    {
        internal string ChannelNumber { get; set; }   // Int32
        internal string TimeStamp { get; set; }   //Int64
        internal string MessageId { get; set; }
        internal string DLC { get; set; }  //Data Length Code. A part of the CAN message. It used to mean simply the length of the CAN message, in bytes, and so had a value between 0 and 8 inclusive. In the revised CAN standard (from 2003) it can take any value between 0 and 15 inclusive, however the length of the CAN message is still limited to at most 8 bytes. All existing CAN controllers will handle a DLC larger than 8.
        internal string MessageValueHex { get; set; }
        internal string MessageValueBinary { get; set; }
        internal string TID { get; set; } // Tx flag, message was transmitted successfully by the CAN controller
        internal string RawCanMessage { get; set; }
    }

    internal class CanMessageProcessor
    {




        // Metody níže semůžou hodit >>>>>


        //internal static BaseCanMessage ConvertMessage(string input)
        //{
        //    string[] subStrings = input.Split(' ');
        //    BaseCanMessage baseCanMessage = new();

        //    baseCanMessage.RawCanMessage = input;

        //    //Channel number
        //    string channelNumberRaw = subStrings[1];
        //    baseCanMessage.ChannelNumber = channelNumberRaw.Substring(channelNumberRaw.IndexOf('=') + 1, channelNumberRaw.Length - 3);

        //    //TimeStamp
        //    string timeStanpRaw = subStrings[2];
        //    baseCanMessage.TimeStamp = timeStanpRaw.Substring(timeStanpRaw.IndexOf('=') + 1, timeStanpRaw.Length - 3);

        //    //MessageId
        //    string messageIdRaw = subStrings[3];
        //    baseCanMessage.MessageId = messageIdRaw.Substring(messageIdRaw.IndexOf('=') + 1, messageIdRaw.Length - 3);

        //    //MessageLenght
        //    string messageLenghtRaw = subStrings[4];
        //    baseCanMessage.DLC = messageLenghtRaw.Substring(messageLenghtRaw.IndexOf('=') + 1, messageLenghtRaw.Length - 3);


        //    string messageValueRaw = subStrings[5];
        //    //MessageValue - HEX
        //    baseCanMessage.MessageValueHex = messageValueRaw;
        //    //MessageValue - BINARY
        //    baseCanMessage.MessageValueBinary = HexToBinary(messageValueRaw);





        //    ////TID 
        //    //string tidRaw = subStrings[6];
        //    //baseCanMessage.TID = tidRaw.Substring(tidRaw.IndexOf('=') + 1, tidRaw.Length - 4);

        //    ////TID
        //    string tidRaw = subStrings[6];
        //    baseCanMessage.TID = tidRaw.Substring(tidRaw.IndexOf('=') + 1, tidRaw.Length - 4);
        //    if (String.Equals(tidRaw, "TX"))
        //    {
        //        tidRaw = subStrings[7];
        //        baseCanMessage.TID = tidRaw.Substring(tidRaw.IndexOf('=') + 1, tidRaw.Length - 4);
        //    }
        //    else
        //    {
        //        baseCanMessage.TID = tidRaw.Substring(tidRaw.IndexOf('=') + 1, tidRaw.Length - 4);
        //    }

        //    return baseCanMessage;
        //}

        //internal static string HexToBinary(string input)
        //{
        //    return string.Join(string.Empty, input.Select(c => Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(4, '0')));
        //}

        //internal static string PartOfMessage(string binaryMessage, int startBit, int lenght)
        //{
        //    if (binaryMessage.Length >= startBit + lenght)
        //    {
        //        return binaryMessage.Substring(startBit, lenght);
        //    }
        //    else
        //    {
        //        return "err";
        //    }
        //}


        //internal string SwapEndianMotorlaToIntel(string input) //  Big to Little
        //{

        //    if (!String.IsNullOrEmpty(input))
        //    {
        //        // String to int32

        //        Int32 intDatatemp = Convert.ToInt32(input);

        //        // Int32 to byte array

        //        byte[] bytesTemp = BitConverter.GetBytes(intDatatemp);

        //        // Check if input is Big - array of bytes reverse

        //        if (!BitConverter.IsLittleEndian)
        //        {
        //            Array.Reverse(bytesTemp);
        //        }

        //        // array bytes to string (int34)


        //        return System.Text.Encoding.Default.GetString(bytesTemp);
        //    }
        //    else
        //    {
        //        throw new("string is null or empty");
        //    }
        //}

    }
}
