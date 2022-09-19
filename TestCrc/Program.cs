namespace TestCrc
{
    internal class Program
    {
        static VectorBusLibrary.Processors.CrcProcessor crc;

        static void Main(string[] args)
        {
            crc = new();
            //Console.WriteLine(crc.GetCrc("0x1AFFD4FF16FFC1"));

            TestCrc();
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
                    Console.WriteLine($"CRC is: {crc.GetCrc(message)}");

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

                int indexFromConsole = Int32.Parse(Console.ReadLine());
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