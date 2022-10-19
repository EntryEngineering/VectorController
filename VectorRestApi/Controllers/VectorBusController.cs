using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using VectorRestApi.Model;
using vxlapi_NET;

namespace VectorRestApi.Controllers
{


    [Route("api/[controller]")]
    [ApiController]
    public class VectorBusController : ControllerBase
    {
        private readonly ILogger<VectorBusController> _logger;

        public VectorBusController(ILogger<VectorBusController> logger)
        {
            _logger = logger;
        }

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
        public ActionResult<List<string>> BusSetup(CanBusConfiguration? canBusConfiguration)
        {
            if (canBusConfiguration != null)
            {
                if (VectorBusApiProcessor.InitCanControloler(canBusConfiguration) == XLDefine.XL_Status.XL_SUCCESS)
                {
                    VectorBusApiProcessor.GetTestMessage();
                    return Ok($"CanBus setup is done");
                }
                else
                {
                    return BadRequest("Error CanBus setup");
                }
            }
            else
            {
                return BadRequest("Error CanBus setup");
            }

        }

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
        public IActionResult SendMessage(MessageModel message)
        {
            VectorBusApiProcessor.SetNewMessage(message);
            return Ok($"Message was send");
        }



        // 4)

        [HttpGet]
        [Route("GetServerState")]
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


        [HttpGet("/ws")]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                _logger.Log(LogLevel.Information, "WebSocket connection established");
                await Echo(webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }

        private async Task Echo(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            _logger.Log(LogLevel.Information, "Message received from Client");

            while (!result.CloseStatus.HasValue)
            {
                var serverMsg = Encoding.UTF8.GetBytes($"Server: Hello. You said: {Encoding.UTF8.GetString(buffer)}");
                await webSocket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
                _logger.Log(LogLevel.Information, "Message sent to Client");

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                _logger.Log(LogLevel.Information, "Message received from Client");

            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            _logger.Log(LogLevel.Information, "WebSocket connection closed");
        }


    }
}
