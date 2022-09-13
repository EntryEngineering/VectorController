using System;
using vxlapi_NET;

namespace VectorBusLibrary.Models
{
    public class CanFdMessageModelRx
    {
        public uint canId { get; set; }  //CAN ID.
        public uint msgFlags { get; set; }
        public uint crc { get; set; }  //Crc of the CAN message.
        public ulong reserved1 { get; set; }
        public ushort totalBitCnt { get; set; }  //Number of received bits including stuff bit.
        public XLDefine.XL_CANFD_DLC dlc { get; set; }  //4-bit data length code.
        public byte reserved { get; set; }  //Internal use.
        public byte[] data { get; set; } = new byte[64];        //Data that was received.

    }

    public class CanFdMessageModelTx 
    {

    }
}
