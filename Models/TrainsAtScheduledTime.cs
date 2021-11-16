/// <summary>
/// Trains with a scheduled time. Used to return the information for the report of what times have multiple incoming trains
/// </summary>

public class TrainsAtScheduledTime
{
    public string? Time { get; set; }

    public HashSet<string>? Trains { get; set; }
}
