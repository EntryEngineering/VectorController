using System;
using System.Collections.Generic;
using System.Linq;

namespace VectorRestApi.Model
{
    public class MessageModel
    {
        public uint MessageId { get; set; }

        public ushort DLC { get; set; }

        public List<SignalModel> Signals { get; set; } = new List<SignalModel>();

        public static SignalModel GetSignal(string signalName, ushort startBit, ushort lengh, string binaryData)
        {
            SignalModel model = new SignalModel();

            if (binaryData.Length == lengh)
            {
                model.SignalName = signalName;
                model.StartBit = startBit;
                model.binaryLengh = lengh;
                model.binaryData = binaryData;
            }
            else
            {
                throw new Exception($"Worng data lengh - Lengh of binaryData is {binaryData.Length} but signal lengh is {lengh} !!");
            }

            return model;
        }



        public static MessageModel SortSignals(MessageModel model)
        {
            List<SignalModel> _sortedSignals = new List<SignalModel>();

            _sortedSignals = model.Signals.OrderBy(x => x.StartBit).ToList();

            MessageModel _model = new MessageModel();
            _model.MessageId = model.MessageId;
            _model.DLC = model.DLC;
            _model.Signals = _sortedSignals;
            return _model;
        }

        public static MessageModel GetMessage(uint messageId, ushort dlc, List<SignalModel> signals)
        {
            List<SignalModel> _sortedSignals = new List<SignalModel>();

            _sortedSignals = signals.OrderBy(x => x.StartBit).ToList();

            MessageModel _model = new MessageModel();
            _model.MessageId = messageId;
            _model.DLC = dlc;
            _model.Signals = _sortedSignals;
            return _model;
        }

    }
}
