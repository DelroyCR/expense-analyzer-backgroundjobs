namespace ExpenseAnalyzer.BackgroundJobs.Api.Configuration;

public sealed class SummarySchedulerOptions
{
    public const string SectionName = "SummaryScheduler";

    public bool Enabled { get; set; } = true;
    public bool RunOnStartup { get; set; } = true;
    public int IntervalSeconds { get; set; } = 60;
    public int ProcessDateOffsetDays { get; set; } = -1;
}