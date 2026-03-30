using ExpenseAnalyzer.BackgroundJobs.Application.Abstractions;
using ExpenseAnalyzer.BackgroundJobs.Infrastructure.Persistence;

namespace ExpenseAnalyzer.BackgroundJobs.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly BackgroundJobsDbContext _context;

    public UnitOfWork(BackgroundJobsDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}