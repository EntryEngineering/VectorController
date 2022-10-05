using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanBusDemoWpf
{
    public class MessageForTx
    {
        public string Byte0;

        public void Run() 
        {
            Trace.WriteLine(Byte0);
        }
    }
}
