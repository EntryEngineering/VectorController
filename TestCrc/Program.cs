namespace TestCrc
{
    internal class Program
    {
        static VectorBusLibrary.Processors.CrcProcessor crc;

        static void Main(string[] args)
        {
            crc = new();

            Console.WriteLine(crc.GetCrc("050300", 0xC3, VectorBusLibrary.Processors.CrcProcessor.Endianness.LittleEndian));
            //Console.WriteLine(crc.GetCrc());





            //1AFFD4FF16FFC1
            //for (int i = 0; i < 30; i++)
            //{
            //    Random random = new Random();
            //    Int64 randomNumber = random.NextInt64(5000000000000000, 9999999999999999);
            //    Console.WriteLine($"Random number {randomNumber.ToString("X")} CRC is:{crc.GetCrc(randomNumber.ToString("X"))}");

            //}


            //TestCrc();
            //TestTable();
            Console.ReadLine();
        }


        private static int TestCrc()
        {
            while (true)
            {
                Console.Write($"Enter message: ");
                string message = Console.ReadLine();
                Console.Write($"Enter s_pdu_kennung: ");
                string s_pdu_kennung = Console.ReadLine();


                string crcResult = $"{crc.GetCrc(message, int.Parse(s_pdu_kennung), VectorBusLibrary.Processors.CrcProcessor.Endianness.LittleEndian)}";
                Console.WriteLine($"CRC is: {crcResult}");
                Console.WriteLine($"Final MSG is: {crcResult}{message}");
            }
        }

        private static void TestTable()
        {
            while (true)
            {

                int indexFromConsole = int.Parse(Console.ReadLine());
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