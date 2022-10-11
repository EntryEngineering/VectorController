namespace VectorRestApi.Model
{
    public class BasicCanBusMessage
    {
        public uint MessageId { get; set; }
        public ushort DLC { get; set; }
        public uint BZ { get; set; }
        public string[] data { get; set; } = new string[32];

    }
}
