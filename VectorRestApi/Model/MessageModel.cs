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
        public long CycleTime { get; set; }


    }
}
