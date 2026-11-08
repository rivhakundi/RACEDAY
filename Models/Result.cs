// Models/Result.cs
using System;

public class Result
{
    public int Id { get; set; }
    public int EnrolmentId { get; set; }
    public Enrolment? Enrolment { get; set; }

    // Time taken for the participant (e.g., in seconds or a TimeSpan)
    public TimeSpan? Time { get; set; }

    // Finishing position within the category/event
    public int? Position { get; set; }

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan FinishTime { get; internal set; }
}