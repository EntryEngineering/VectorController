using System.Collections.Generic;

namespace VectorRestApi.Model
{
    public class MessageModel
    {
        public uint MessageId { get; set; }

        public ushort DLC { get; set; }

        public List<SignalModel> Signals { get; set; }
    }
}
