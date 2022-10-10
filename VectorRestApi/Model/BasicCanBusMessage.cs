namespace VectorRestApi.Model
{
    public class BasicCanBusMessage
    {
        public uint MessageId { get; set; }
        public ushort DLC { get; set; }
        public string data0Byte { get; set; }
        public string data1Byte { get; set; }
        public string data2Byte { get; set; }
        public string data3Byte { get; set; }
        public string data4Byte { get; set; }
        public string data5Byte { get; set; }
        public string data6Byte { get; set; }
        public string data7Byte { get; set; }

    }
}
