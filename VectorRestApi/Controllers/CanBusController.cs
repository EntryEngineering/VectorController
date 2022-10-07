using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VectorBusLibrary.Processors;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace VectorRestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CanBusController : ControllerBase
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


        [HttpGet]
        public string Get()
        {
            return "sd";
        }

        // GET api/<CanBusController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<CanBusController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<CanBusController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<CanBusController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
