using ExpenseAnalyzer.BackgroundJobs.Domain.Entities;

namespace ExpenseAnalyzer.BackgroundJobs.Application.Abstractions;

public interface ISummarySnapshotRepository
{
    Task<SummarySnapshot?> GetByUserAndPeriodAsync(
        Guid userId,
        DateTime periodStartUtc,
        DateTime periodEndUtc,
        CancellationToken cancellationToken = default);

    Task AddAsync(SummarySnapshot snapshot, CancellationToken cancellationToken = default);

    void Update(SummarySnapshot snapshot);
}