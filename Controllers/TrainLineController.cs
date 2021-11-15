using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TrainSchedule.Models;
using TrainSchedule.Database;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TrainSchedule.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class TrainLineController : ControllerBase
    {
        private readonly ILogger<TrainLineController> _logger;
        private readonly IDb _db;

        public TrainLineController(ILogger<TrainLineController> logger, IDb db)
        {
            _logger = logger;
            _db = db;
        }

        private string Key(string name)
        {
            return $"TL-{name}";
        }

        // GET: api/<TrainLineController>
        [HttpGet]
        [Route("{name}")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<ActionResult<TrainLine>> Get(string name)
        {
            try
            {
                var key = Key(name);
                var json = (await _db.Read(key)).AsString();
                var tls = JsonConvert.DeserializeObject<TrainLine>(json);
                return Ok(tls);
            } catch (KeyNotFoundException)
            {
                return NotFound(name);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST api/<TrainLineController>
        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<ActionResult> Post([FromBody] TrainLine line)
        {
            try
            {
                if (line == null || line.Name == null) throw new NullReferenceException();
                var json = JsonConvert.SerializeObject(line);
                var key = Key(line.Name);
                await _db.Write(key, json);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE api/<TrainLineController>/name
        [HttpDelete("{name}")]
        public async Task Delete(string name)
        {
            var key = Key(name);
            await _db.Delete(key);
        }
    }
}
