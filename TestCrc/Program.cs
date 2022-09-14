namespace TestCrc
{
    internal class Program
    {
        static void Main(string[] args)
        {
            VectorBusLibrary.Processors.CrcProcessor crc = new();
            Console.WriteLine(crc.GetCrc("0x1AFFD4FF16FFC1"));
        }
    }
}