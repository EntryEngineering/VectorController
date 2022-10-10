using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using VectorBusLibrary.Processors;
using vxlapi_NET;
using static vxlapi_NET.XLDefine;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace VectorRestApi.Controllers
{
    

    [Route("api/[controller]")]
    [ApiController]
    public class VectorBusController : ControllerBase
    {

        //TODO:
        // 1) [POST] inicializace Vector prevodníku s paramtery jako:
        //      - bus type (Can nebo CanFD)
        //      - baudrate
        //      - číslo kanálu
        // odpveď > úspěšné probedení openDriver,openChannel,openPort atd

        // 2) [POST] Zapnutí odesílání s testovací zprávou

        // 3) [UPDATE] Nastavení nové zprávy (upravené jen některé hodnoty)

        // 4) [GET] Záskání stavu o běhu odesílání

        // 5) [UPDATE] Zastavení odesílání

        //-------------------------------------------------------------------------------------------------


        
        // 1)
        // V core vytvořit datový model BusConfig kde budou všechny potřebné parametry jako argument
        [HttpPost]
        [Route("BusSetup")]
        public IActionResult BusSetup() 
        {
            // všechny akce pro nastavení

            InitCanControloler();

            return Ok($"CanBus setup is done");
        }


        // 2)
        

        [HttpPost]
        [Route("StartTx")]
        public IActionResult StartTx()
        {
            StartTxLoop();
            Trace.WriteLine("TX START");
            return Ok($"Tx loop start with test message");
        }

        

        // 3)
        // jako agrument datový model zprávy
        [HttpPut]
        [Route("SendMessage")]
        public IActionResult SendMssage()
        {

            return Ok($"Message was send");
        }


        // 4)

        [HttpGet]
        [Route("GetTxState")]
        public IActionResult GetTxState()
        {

            return Ok($"Tx loop state is: ");
        }


        // 5)

        [HttpPut]
        [Route("StopTx")]
        public IActionResult Stoptx()
        {

            StopTxLoop();
            Trace.WriteLine("TX STOPED");
            return Ok($"Tx lool stoped");
        }


        // API dokumentace

        [HttpGet]
        [Route("ApiInfo")]
        public IActionResult ApiInfo()
        {
            string info = "Can Bus and CanFD Bus Api";

            return Ok(info);
        }


        public Timer txTimer;

        internal void InitTxLoop() 
        {
            txTimer = new Timer();
            txTimer.Elapsed += TimerForTx_Elapsed;
            txTimer.AutoReset = true;
            txTimer.Interval = 100;
            txTimer.Enabled = true;
        }

        internal void StartTxLoop() 
        {
            InitTxLoop();
            txTimer.Start();
        }

        internal void StopTxLoop() 
        {
            txTimer.Stop();
        }




        //-------------------------------------------------------------

        private XLClass.xl_event_collection GetTestMessgae()
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

        private void TimerForTx_Elapsed(object sender, ElapsedEventArgs e)
        {
            XL_Status status = canBus.CanTransmit(GetTestMessgae());
        }

        
        private static CanBus canBus;

        internal static void InitCanControloler()
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
