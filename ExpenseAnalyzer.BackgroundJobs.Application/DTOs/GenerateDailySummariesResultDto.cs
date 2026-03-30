namespace ExpenseAnalyzer.BackgroundJobs.Application.DTOs;

public class GenerateDailySummariesResultDto
{
    public Guid ExecutionId { get; set; }
    public string JobName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int ProcessedUsers { get; set; }
    public int GeneratedSnapshots { get; set; }
    public DateTime DateUtc { get; set; }
}