/// <summary>
/// A single trains schedule
/// </summary>

public class TrainSchedule
{
    public TrainSchedule()
    {
        Times = new();
    }

    public HashSet<string> Times { get; set; }
}
