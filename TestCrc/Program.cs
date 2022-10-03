namespace TestCrc
{
    internal class Program
    {
        static VectorBusLibrary.Processors.CrcProcessor crc;

        static void Main(string[] args)
        {
            crc = new();
            Console.WriteLine(crc.GetCrc("000300", VectorBusLibrary.Processors.CrcProcessor.Endianness.LittleEndian));
            //1AFFD4FF16FFC1
            //for (int i = 0; i < 30; i++)
            //{
            //    Random random = new Random();
            //    Int64 randomNumber = random.NextInt64(5000000000000000, 9999999999999999);
            //    Console.WriteLine($"Random number {randomNumber.ToString("X")} CRC is:{crc.GetCrc(randomNumber.ToString("X"))}");

            //}


            //TestCrc();
            Console.ReadLine();
        }


        private static int TestCrc()
        {
            while (true)
            {
                Console.Write($"Enter message: ");
                string message = Console.ReadLine();
                if (message.Length == 14)
                {
                    Console.WriteLine($"CRC is: {crc.GetCrc(message, VectorBusLibrary.Processors.CrcProcessor.Endianness.LittleEndian)}");
                }
                else
                {
                    Console.WriteLine($"Out of range");
                }

            }
        }

        private static void TestTable()
        {
            while (true)
            {

                UInt32 indexFromConsole = uint.Parse(Console.ReadLine());
                if (indexFromConsole <= 255)
                {
                    Console.WriteLine($"Result is {string.Format("0x{0:x}", crc.GetValueFromTable(indexFromConsole))}");

                }
                else
                {
                    Console.WriteLine("Out of range");
                }
            }
        }
    }
}