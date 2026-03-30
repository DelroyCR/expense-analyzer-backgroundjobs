namespace ExpenseAnalyzer.BackgroundJobs.Domain.Entities;

public class SourceTransaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
}