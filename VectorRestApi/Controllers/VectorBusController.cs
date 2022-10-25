using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
            if (VectorBusApiProcessor.InitCanDone == true)
            {
                VectorBusApiProcessor.StartTxLoop();
                Trace.WriteLine("TX START");
                return Ok($"Tx loop start with test message");
            }
            else
            {
                return BadRequest($"The setting of CanBus has not been done");
            }
        }

        [HttpPost]
        [Route("SendMessageWithCrc")]
        public IActionResult SendMessageCrc(MessageModel message)
        {
            if (VectorBusApiProcessor.InitCanDone == true)
            {
                string result = VectorBusApiProcessor.CheckDlcAndBinaryLenghOfInsertingMessage(message.Message, message.DLC, true);
                if (result == "OK")
                {
                    VectorBusApiProcessor.SetNewMessage(message, true);
                    return Ok($"Message was send - {result}");
                }
                else
                {
                    return BadRequest(result);
                }
            }
            else
            {
                return BadRequest($"The setting of CanBus has not been done");
            }
        }


        [HttpPost]
        [Route("SendMessageWithoutCrc")]
        public IActionResult SendMessageNonCrc(MessageModel message)
        {
            if (VectorBusApiProcessor.InitCanDone == true)
            {
                string result = VectorBusApiProcessor.CheckDlcAndBinaryLenghOfInsertingMessage(message.Message, message.DLC, false);
                if (result == "OK")
                {
                    VectorBusApiProcessor.SetNewMessage(message, false);
                    return Ok($"Message was send - {result}");
                }
                else
                {
                    return BadRequest(result);
                }
            }
            else
            {
                return BadRequest($"The setting of CanBus has not been done");
            }
        }

        [HttpGet]
        [Route("GetServerState")]
        public IActionResult GetTxState()
        {
            return Ok($"Tx loop state is: {VectorBusApiProcessor.InitCanDone}");
        }

        [HttpGet]
        [Route("StopTx")]
        public IActionResult StopTx()
        {
            if (VectorBusApiProcessor.InitCanDone == true)
            {
                VectorBusApiProcessor.StopTxLoop();
                Trace.WriteLine("TX STOPED");
                return Ok($"Tx lool stoped");
            }
            else
            {
                return BadRequest($"The setting of CanBus has not been done");
            }
        }

        //[HttpGet("/ws")]
        //public async Task Get()
        //{
        //    if (HttpContext.WebSockets.IsWebSocketRequest)
        //    {
        //        using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        //        _logger.Log(LogLevel.Information, "WebSocket connection established");
        //        await Echo(webSocket);
        //    }
        //    else
        //    {
        //        HttpContext.Response.StatusCode = 400;
        //    }
        //}

        //private async Task Echo(WebSocket webSocket)
        //{
        //    var buffer = new byte[1024 * 4];
        //    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        //    _logger.Log(LogLevel.Information, "Message received from Client");

        //    while (!result.CloseStatus.HasValue)
        //    {
        //        var serverMsg = Encoding.UTF8.GetBytes($"Server: Hello. You said: {Encoding.UTF8.GetString(buffer)}");
        //        await webSocket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
        //        _logger.Log(LogLevel.Information, "Message sent to Client");

        //        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        //        _logger.Log(LogLevel.Information, "Message received from Client");

        //    }
        //    await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        //    _logger.Log(LogLevel.Information, "WebSocket connection closed");
        //}

    }
}
