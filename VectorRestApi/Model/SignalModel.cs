﻿namespace VectorRestApi.Model
{
    public class SignalModel
    {
        public string SignalName { get; set; }
        public ushort StartBit { get; set; }
        public ushort binaryLengh { get; set; }
        public string binaryData { get; set; }
    }
}
