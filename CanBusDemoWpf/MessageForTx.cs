using System.Diagnostics;

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
