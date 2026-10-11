namespace RaceDay.Api.Models;

public class Result
{
    public int ResultId { get; set; }

    public int EnrolmentId { get; set; }
    public Enrolment Enrolment { get; set; } = null!;

    public TimeOnly? FinishTime { get; set; }
    public int? FinishingPosition { get; set; }

    public DateTime CapturedAt { get; set; } = DateTime.UtcNow;
}
