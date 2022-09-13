using System;
using VectorBusLibrary.Processors;
using vxlapi_NET;

namespace CanFdBusDemoConsole
{
    internal class Program
    {
        private static CanFdBus canFdBus;
        static void Main(string[] args)
        {
            canFdBus = new CanFdBus(new XLDriver(), XLDefine.XL_HardwareType.XL_HWTYPE_VN1610, "CanFD Bus-Test");
            InitCanControloler();
            canFdBus.RunRxThread();
            while (true)
            {
                var data1 = canFdBus.CanFdBusMessageRx.data[1];
                var data2 = canFdBus.CanFdBusMessageRx.data[2];
                Console.WriteLine($"Knob_{data1}-Klema_{data2}");
            }

        }

        internal static void InitCanControloler()
        {
            Console.WriteLine("****************************");
            Console.WriteLine("CanFDBus - Vector");
            Console.WriteLine("****************************");

            Console.WriteLine("vxlapi_NET        : " + typeof(XLDriver).Assembly.GetName().Version);
            canFdBus.OpenDriver();
            canFdBus.GetDriverConfig();
            canFdBus.GetAppConfigAndSetAppConfig();
            canFdBus.RequestTheUserToAssignChannels();
            CommonVector.GetAccesMask();
            Console.WriteLine(CommonVector.PrintAccessMask());
            canFdBus.OpenPort();
            canFdBus.SetCanFdConfiguration();
            canFdBus.SetNotificationCanFdBus();
            canFdBus.ActivateChannel();
            canFdBus.GetXlDriverConfiguration();
        }
    }
}