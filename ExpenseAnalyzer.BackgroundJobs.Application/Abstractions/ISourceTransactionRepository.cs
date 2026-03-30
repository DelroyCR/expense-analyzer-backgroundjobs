using ExpenseAnalyzer.BackgroundJobs.Domain.Entities;

namespace ExpenseAnalyzer.BackgroundJobs.Application.Abstractions;

public interface ISourceTransactionRepository
{
    Task<List<SourceTransaction>> GetByDateRangeAsync(
        DateTime periodStartUtc,
        DateTime periodEndUtc,
        Guid? userId,
        CancellationToken cancellationToken = default);
}