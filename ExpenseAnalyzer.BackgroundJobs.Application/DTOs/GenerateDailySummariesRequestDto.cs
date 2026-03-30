namespace ExpenseAnalyzer.BackgroundJobs.Application.DTOs;

public class GenerateDailySummariesRequestDto
{
    public DateTime DateUtc { get; set; }
    public Guid? UserId { get; set; }
}