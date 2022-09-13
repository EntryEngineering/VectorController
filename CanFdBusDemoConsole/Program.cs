using System;
using VectorBusLibrary.Processors;
using vxlapi_NET;

namespace CanFdBusDemoConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CanFdBus canFdBus = new CanFdBus(new XLDriver(), XLDefine.XL_HardwareType.XL_HWTYPE_VN1610, "CanFD Bus-Test");
            canFdBus.TestCanFDBus();
        }
    }
}