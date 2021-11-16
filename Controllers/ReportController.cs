using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

/// <summary>
/// Report controller.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ReportController : ControllerBase
{
    private readonly ILogger<TrainLineController> _logger;
    private readonly IDb _db;

    public ReportController(ILogger<TrainLineController> logger, IDb db)
    {
        _logger = logger;
        _db = db;
    }

    private TrainsAtScheduledTime? GetAnswer(List<(string Name, int Time)> list, int time)
    {
        var answer =
            list
                .Where(i => i.Time > time)
                .GroupBy(a => a.Time)
                .Where(i => i.Count() > 1)
                .OrderBy(i => i.Key)
                .FirstOrDefault();

        if (answer != null && answer.Any())
        {
            var mt = new TrainsAtScheduledTime
            {
                Time = IntToTime(answer.Key, true),
                Trains = (from ti in answer orderby ti.Name select ti.Name).ToHashSet()
            };
            return mt;
        }

        // if we didn't find anything in the user limited search then search from 0 (whole day)
        if (time != 0)
        {
            return GetAnswer(list, 0);
        }

        return null;
    }

    /// <summary>
    /// Call with a time value (1:34 pm).
    /// Returns the first time that more than one train is in the station.
    /// </summary>
    /// <param name="time">Time, in hh:mm [am|pm] format</param>
    /// <returns></returns>
    [HttpGet]
    [Route("{time}")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public async Task<ActionResult<TrainsAtScheduledTime>> Get(string time)
    {
        try
        {
            var askForTime = TimeToInt(time);
            List<(string Name, int Time)> allTimes = new();
            var trainLineNames = from item in await _db.Keys() where item.StartsWith("TL-") select item;
            foreach (var trainLineName in trainLineNames)
            {
                var json = (await _db.Fetch(trainLineName)).AsString();
                var trainLine = JsonConvert.DeserializeObject<TrainLine>(json);
                if (trainLine is not null && trainLine.Name is not null && trainLine.Schedule is not null)
                {
                    foreach (var scheduleTime in trainLine.Schedule.Times)
                    {
                        var scheduleInt = TimeToInt(scheduleTime);
                        allTimes.Add(new(trainLine.Name, scheduleInt));
                    }
                }
            }

            var answer = GetAnswer(allTimes, askForTime);

            if (answer == null)
            {
                return NotFound();
            }
            return Ok(answer);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private static int TimeToInt(string time)
    {
        time = time ?? throw new NullReferenceException();
        var s = time.Split(new[] { ':', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (s.Length < 2)
            throw new InvalidOperationException("Time format is invalid, should be: hh:mm [am|pm]");
        var hourText = s[0];
        var minuteText = s[1];

        if (hourText.Any(c => !char.IsDigit(c)))
            throw new InvalidOperationException("Time values should only contain numbers, example: hh:mm [am|pm]");

        if (minuteText.Any(c => !char.IsDigit(c)))
            throw new InvalidOperationException("Time values should only contain numbers, example: hh:mm [am|pm]");

        if (!int.TryParse(hourText, out var hourValue))
            throw new InvalidOperationException("Time value appears to be incorrect, should only contain numbers, example: hh:mm [am|pm]");

        if (hourValue > 24)
            throw new InvalidCastException("hour cannot be greater than 24.");

        if (!int.TryParse(minuteText, out var minuteValue))
            throw new InvalidOperationException("Time value appears to be incorrect, should only contain numbers, example: hh:mm [am|pm]");

        var ampm = "am";
        if (s.Length == 3)
            ampm = s[2].Trim().ToLower();

        if (ampm == "pm")
        {
            if (hourValue > 12)
                throw new InvalidCastException("pm passed in but hour value is greater than 12.");

            hourValue += 12;
        }

        if (minuteValue > 59)
            throw new InvalidCastException("Invalid time format, minute value is greater than 59.");

        return hourValue * 60 + minuteValue;
    }

    private static string IntToTime(int value, bool ampm)
    {
        var hour = value / 60;
        var minutes = value % 60;
        var ampmText = "am";

        if (ampm)
        {
            if (hour > 12) hour -= 12;
            ampmText = "pm";
        }

        return $"{hour}:{minutes}{(ampm ? $" {ampmText}" : "")}";
    }


}
