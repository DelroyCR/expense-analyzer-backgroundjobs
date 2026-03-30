using ExpenseAnalyzer.BackgroundJobs.Application.Abstractions;
using ExpenseAnalyzer.BackgroundJobs.Domain.Entities;
using ExpenseAnalyzer.BackgroundJobs.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseAnalyzer.BackgroundJobs.Infrastructure.Repositories;

public class SummarySnapshotRepository : ISummarySnapshotRepository
{
    private readonly BackgroundJobsDbContext _context;

    public SummarySnapshotRepository(BackgroundJobsDbContext context)
    {
        _context = context;
    }

    public async Task<SummarySnapshot?> GetByUserAndPeriodAsync(
        Guid userId,
        DateTime periodStartUtc,
        DateTime periodEndUtc,
        CancellationToken cancellationToken = default)
    {
        return await _context.SummarySnapshots
            .FirstOrDefaultAsync(
                x => x.UserId == userId
                  && x.PeriodStartUtc == periodStartUtc
                  && x.PeriodEndUtc == periodEndUtc,
                cancellationToken);
    }

    public async Task AddAsync(SummarySnapshot snapshot, CancellationToken cancellationToken = default)
    {
        await _context.SummarySnapshots.AddAsync(snapshot, cancellationToken);
    }

    public void Update(SummarySnapshot snapshot)
    {
        _context.SummarySnapshots.Update(snapshot);
    }
}