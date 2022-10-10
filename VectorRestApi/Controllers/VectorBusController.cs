using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

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

            VectorBusApiProcessor.InitCanControloler();

            return Ok($"CanBus setup is done");
        }


        // 2)
        

        [HttpPost]
        [Route("StartTx")]
        public IActionResult StartTx()
        {
            VectorBusApiProcessor.StartTxLoop();
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

            VectorBusApiProcessor.StopTxLoop();
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


       


    }
}
