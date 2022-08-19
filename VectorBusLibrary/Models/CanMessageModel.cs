namespace VectorBusLibrary.Models
{
    internal class CanBusRx
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


    internal class CanBusTX
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
}
