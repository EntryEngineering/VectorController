namespace VectorController.Models
{

    internal class VectorDeviceInfo
    {
        public string ChannelName { get; set; }
        public ulong ChannelMask { get; set; }
        public string TransceiverName { get; set; }
        public uint SerialNumber { get; set; }
    }
}
