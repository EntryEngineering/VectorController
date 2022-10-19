using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using vxlapi_NET;

namespace VectorRestApi.Model
{
    public class MessageModel
    {
        public uint MessageId { get; set; }

        public ushort DLC { get; set; }

        public string Message { get; set; }
        


        //public List<SignalModel> Signals { get; set; } = new List<SignalModel>();

        //public static SignalModel GetSignal(string signalName, ushort startBit, ushort lengh, ushort[] binaryData)
        //{
        //    SignalModel model = new SignalModel();

        //    if (binaryData.Length == lengh)
        //    {
        //        model.SignalName = signalName;
        //        model.StartBit = startBit;
        //        model.binaryLengh = lengh;
        //        model.binaryData = binaryData;
        //    }
        //    else
        //    {
        //        throw new Exception($"Worng data lengh - Lengh of binaryData is {binaryData.Length} but signal lengh is {lengh} !!");
        //    }

        //    return model;
        //}

        //public static XLClass.xl_event_collection ConvertToTxFormat(MessageModel model) 
        //{
        //    if (true)
        //    {

        //    }

        //    XLClass.xl_event_collection _Event_Collection = new XLClass.xl_event_collection(1);
        //    _Event_Collection.xlEvent[0].tagData.can_Msg.id = model.MessageId;
        //    _Event_Collection.xlEvent[0].tagData.can_Msg.dlc = model.DLC;
        //    _Event_Collection.xlEvent[0].tagData.can_Msg.data = new byte[8];

        //    return _Event_Collection;
        //}
    }
}
