using ExpenseAnalyzer.BackgroundJobs.Application.Abstractions;
using ExpenseAnalyzer.BackgroundJobs.Domain.Entities;
using ExpenseAnalyzer.BackgroundJobs.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseAnalyzer.BackgroundJobs.Infrastructure.Repositories;

public class SourceTransactionRepository : ISourceTransactionRepository
{
    private readonly BackgroundJobsDbContext _context;

    public SourceTransactionRepository(BackgroundJobsDbContext context)
    {
        _context = context;
    }

    public async Task<List<SourceTransaction>> GetByDateRangeAsync(
        DateTime periodStartUtc,
        DateTime periodEndUtc,
        Guid? userId,
        CancellationToken cancellationToken = default)
    {
        var query = _context.SourceTransactions
            .AsNoTracking()
            .Where(t => t.Date >= periodStartUtc && t.Date < periodEndUtc);

        if (userId.HasValue)
        {
            query = query.Where(t => t.UserId == userId.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }
}