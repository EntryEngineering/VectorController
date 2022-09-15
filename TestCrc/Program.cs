namespace TestCrc
{
    internal class Program
    {
        static VectorBusLibrary.Processors.CrcProcessor crc;

        static void Main(string[] args)
        {
            crc = new();
            //Console.WriteLine(crc.GetCrc("0x1AFFD4FF16FFC1"));

            Console.WriteLine($"{crc.GetCrc("1AFFD4FF16FFC1")}");
            TestTable();
            Console.ReadLine();
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