namespace ExpenseAnalyzer.BackgroundJobs.Domain.Entities;

public class SummarySnapshot
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public DateTime PeriodStartUtc { get; set; }
    public DateTime PeriodEndUtc { get; set; }

    public decimal IncomeTotal { get; set; }
    public decimal ExpenseTotal { get; set; }
    public decimal NetTotal { get; set; }

    public int TransactionCount { get; set; }

    public DateTime GeneratedAtUtc { get; set; }
}