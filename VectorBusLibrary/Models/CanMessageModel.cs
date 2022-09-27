namespace VectorBusLibrary.Models
{
    public class CanBusRx
    {
        public ulong TimeStamp { get; set; }
        public uint MessageId { get; set; }
        public ushort DLC { get; set; }  //Data Length Code. A part of the CAN message. It used to mean simply the length of the CAN message, in bytes, and so had a value between 0 and 8 inclusive. In the revised CAN standard (from 2003) it can take any value between 0 and 15 inclusive, however the length of the CAN message is still limited to at most 8 bytes. All existing CAN controllers will handle a DLC larger than 8.
        public byte[] data { get; set; } = new byte[64];
        public string RawCanMessage { get; set; }

    }


    public class CanBusTx
    {



    }
}
