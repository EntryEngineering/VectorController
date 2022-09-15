namespace TestCrc
{
    internal class Program
    {
        static void Main(string[] args)
        {
            VectorBusLibrary.Processors.CrcProcessor crc = new();
            //Console.WriteLine(crc.GetCrc("0x1AFFD4FF16FFC1"));

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