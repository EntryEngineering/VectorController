using System;
using VectorBusLibrary.Processors;
using vxlapi_NET;

namespace CanBusDemoConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CanBus canBus = new(new XLDriver(), XLDefine.XL_HardwareType.XL_HWTYPE_VN1610, "Can Bus-Test");
            canBus.TestCanBus();
        }
    }
}