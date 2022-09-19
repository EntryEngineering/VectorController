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

            Console.WriteLine("Press key for next step:");
            Console.WriteLine("Key 'r' for run Rx");
            Console.WriteLine("Key 't' for run Tx");
            string? pressedKey = Console.ReadLine();
            while (true)
            {
                if (pressedKey == "r")
                {
                    Console.WriteLine("Set count and press enter..");
                    Int64 count = Int64.Parse(Console.ReadLine());
                    canFdBus.RunRxThread();
                    for (int i = 0; i < count; i++)
                    {
                        var data1 = canFdBus.CanFdBusMessageRx.data[1];
                        var data2 = canFdBus.CanFdBusMessageRx.data[2];
                        Console.WriteLine($"Knob_{data1}-Klema_{data2} CRC: {canFdBus.CanFdBusMessageRx.crc} RAW: {canFdBus.CanFdBusMessageRx.RawDataString }");
                    }
                    Console.WriteLine("Rx end");

                }
                else if (pressedKey == "t")
                {
                    XLClass.xl_canfd_event_collection messageForTransmit = new XLClass.xl_canfd_event_collection(1);
                    messageForTransmit.xlCANFDEvent[0].tagData.canId = 0x3C0;
                    messageForTransmit.xlCANFDEvent[0].tagData.data[1] = 5;

                    canFdBus.CanFdTransmit(messageForTransmit);

                }
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