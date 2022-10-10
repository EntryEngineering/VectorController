using System;
using System.Diagnostics;
using System.Timers;
using VectorBusLibrary.Processors;
using vxlapi_NET;
using static vxlapi_NET.XLDefine;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace VectorRestApi.Controllers
{
    public static class VectorBusApiProcessor 
    {
        //-------------------------------------------------------------

        private static XLClass.xl_event_collection GetTestMessgae()
        {
            XLClass.xl_event_collection xlEventCollection = new XLClass.xl_event_collection(1);
            xlEventCollection.xlEvent[0].tagData.can_Msg.id = 0x3C0;
            xlEventCollection.xlEvent[0].tagData.can_Msg.dlc = 4;
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[0] = Convert.ToByte("7A", 16);
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[1] = Convert.ToByte("AA", 16);
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[2] = Convert.ToByte("BB", 16);
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[3] = Convert.ToByte("CC", 16);
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[4] = Convert.ToByte("DD", 16);
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[5] = Convert.ToByte("FF", 16);
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[6] = Convert.ToByte("15", 16);
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[7] = Convert.ToByte("51", 16);

            xlEventCollection.xlEvent[0].tag = XL_EventTags.XL_TRANSMIT_MSG;

            return xlEventCollection;
        }


        public static void InitTxLoop()
        {
            txTimer.Elapsed += TimerForTx_Elapsed;
            txTimer.AutoReset = true;
            txTimer.Interval = 100;

        }

        public static void StartTxLoop()
        {
            InitTxLoop();
            txTimer.Enabled = true;
        }

        public static void StopTxLoop()
        {
            txTimer.Enabled = false;
        }


        private static Timer txTimer = new Timer();
        private static void TimerForTx_Elapsed(object sender, ElapsedEventArgs e)
        {
            XL_Status status = canBus.CanTransmit(GetTestMessgae());
        }


        private static CanBus canBus;

        public static void InitCanControloler()
        {
            canBus = new(XLDefine.XL_HardwareType.XL_HWTYPE_VN1610, "VectorCanBus_RestApi");
            Trace.WriteLine("****************************");
            Trace.WriteLine("CanBus - Vector");
            Trace.WriteLine("****************************");

            Trace.WriteLine("vxlapi_NET        : " + typeof(XLDriver).Assembly.GetName().Version);
            canBus.OpenDriver();
            canBus.GetDriverConfig();
            canBus.GetAppConfigAndSetAppConfig();
            canBus.RequestTheUserToAssignChannels();
            CommonVector.GetAccesMask();
            Trace.WriteLine(CommonVector.PrintAccessMask());
            canBus.OpenPort();
            canBus.ActivateChannel();
            canBus.SetNotificationCanBus();
            canBus.ResetClock();
        }
    }
}
