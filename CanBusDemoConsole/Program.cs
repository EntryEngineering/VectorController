using VectorBusLibrary.Processors;
using vxlapi_NET;

namespace CanBusDemoConsole
{
    internal class Program
    {
        private static CanBus canBus;

        static void Main(string[] args)
        {
            canBus = new(new XLDriver(), XLDefine.XL_HardwareType.XL_HWTYPE_VN1610, "Can Bus-Test");
            InitCanControloler();
            canBus.RunRxThread();
            Console.WriteLine("RX msg:");

            while (true)
            {

                //Console.WriteLine(canBus.msgTestOut);
                Console.WriteLine($"{canBus.CanBusMessageRx.RawCanMessage} --- Thread :{Thread.CurrentThread.ManagedThreadId.ToString()}");


            }
        }



        internal static void InitCanControloler()
        {
            Console.WriteLine("****************************");
            Console.WriteLine("CanBus - Vector");
            Console.WriteLine("****************************");

            Console.WriteLine("vxlapi_NET        : " + typeof(XLDriver).Assembly.GetName().Version);
            canBus.OpenDriver();
            canBus.GetDriverConfig();
            canBus.GetAppConfigAndSetAppConfig();
            canBus.RequestTheUserToAssignChannels();
            CommonVector.GetAccesMask();
            Console.WriteLine(CommonVector.PrintAccessMask());
            canBus.OpenPort();
            canBus.ActivateChannel();
            canBus.SetNotificationCanBus();
            canBus.ResetClock();
        }
    }
}