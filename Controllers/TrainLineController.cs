using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

/// <summary>
/// API controller for CRUD operations on trainlines
/// </summary>
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

    /// <summary>
    /// The key is prefaced by "TL" (TrainLine) which makes it easier to find
    /// files in the filesystem and allows for storing other types.
    /// </summary>
    /// <param name="name">key name</param>
    /// <returns>transformed key name</returns>
    public static string Key(string name)
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
            var json = (await _db.Fetch(key)).AsString();
            var tls = JsonConvert.DeserializeObject<TrainLine>(json);
            return Ok(tls);
        }
        catch (KeyNotFoundException)
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
            if (line == null || string.IsNullOrEmpty(line.Name))
                throw new NullReferenceException();

            if (line.Name.All(char.IsLetterOrDigit) && line.Name.Length <= 4 && line.Name.Length > 0)
            {
                var  json = JsonConvert.SerializeObject(line);
                var key = Key(line.Name);
                await _db.Set(key, json);
                return Ok();
            }
            else
                return BadRequest($"Train line name '{line.Name}' must be 1 to 4 alpha numeric characters.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Not officially called for readme.md but a nice to have.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    // DELETE api/<TrainLineController>/name
    [HttpDelete("{name}")]
    public async Task Delete(string name)
    {
        var key = Key(name);
        await _db.Delete(key);
    }
}
