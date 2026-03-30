namespace ExpenseAnalyzer.BackgroundJobs.Domain.Entities;

public class BackgroundJobExecution
{
    public Guid Id { get; set; }

    public string JobName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public DateTime StartedAtUtc { get; set; }
    public DateTime? FinishedAtUtc { get; set; }

    public string? ErrorMessage { get; set; }

    public int AttemptCount { get; set; }

    public string TriggeredBy { get; set; } = string.Empty;
}