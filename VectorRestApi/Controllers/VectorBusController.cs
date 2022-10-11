using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics;
using VectorRestApi.Model;
using vxlapi_NET;

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
        public ActionResult<List<string>> BusSetup()
        {

            if (VectorBusApiProcessor.InitCanControloler() == XLDefine.XL_Status.XL_SUCCESS)
            {
                return Ok($"CanBus setup is done");
            }
            else
            {
                return BadRequest("Error CanBus setup");
            }
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
        [HttpPost]
        [Route("SendMessage")]
        public IActionResult SendMessage(BasicCanBusMessage message)
        {
            VectorBusApiProcessor.SetNewMessage(message);
            return Ok($"Message was send");
        }


        // 4)

        [HttpGet]
        [Route("GetTxState")]
        public IActionResult GetTxState()
        {

            return Ok($"Tx loop state is: {VectorBusApiProcessor.InitCanDone}");
        }


        // 5)

        [HttpGet]
        [Route("StopTx")]
        public IActionResult StopTx()
        {
            VectorBusApiProcessor.StopTxLoop();
            Trace.WriteLine("TX STOPED");
            return Ok($"Tx lool stoped");
        }


        //// API dokumentace

        //[HttpGet]
        //[Route("ApiInfo")]
        //public IActionResult ApiInfo()
        //{
        //    string info = "Can Bus and CanFD Bus Api";

        //    return Ok(info);
        //}





    }
}
