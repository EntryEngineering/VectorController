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
                    canBus.RunRxThread();
                    for (int i = 0; i < count; i++)
                    {
                        byte data1 = canBus.CanBusMessageRx.data[1];
                        byte data2 = canBus.CanBusMessageRx.data[2];
                        string stringToPrint = $"Knob_{data1}-Klema_{data2}";
                        Console.WriteLine(stringToPrint);
                        FileProcessor.SaveTextToFileAsync(stringToPrint);

                        //Console.WriteLine($"Raw: {canBus.CanBusMessageRx.RawCanMessage}");
                        
                    }
                    Console.WriteLine("Rx end");

                }
                else if (pressedKey == "t")
                {
                    XLClass.xl_event_collection messageForTransmit = new XLClass.xl_event_collection(1);
                    messageForTransmit.xlEvent[0].tagData.can_Msg.id = 0x3C0;
                    messageForTransmit.xlEvent[0].tagData.can_Msg.data[1] = 7;

                    canBus.CanTransmit(messageForTransmit);
                }
                else if (pressedKey == "x")
                {
                    canBus.RunRxThread();
                    while (true)
                    {

                        for (int i = 0; i < canBus.CanBusMessageRx.DLC; i++)
                        {
                            Console.Write(canBus.CanBusMessageRx.data[i].ToString("x"));

                        }
                        Console.WriteLine("*");

                        //Console.Write($"{canBus.CanBusMessageRx.RawCanMessage}");
                        //Console.WriteLine("*");
                    }
                }
            }


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