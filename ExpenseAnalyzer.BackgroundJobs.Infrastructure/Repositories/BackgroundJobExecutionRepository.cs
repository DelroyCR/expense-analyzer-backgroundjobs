using ExpenseAnalyzer.BackgroundJobs.Application.Abstractions;
using ExpenseAnalyzer.BackgroundJobs.Domain.Entities;
using ExpenseAnalyzer.BackgroundJobs.Infrastructure.Persistence;

namespace ExpenseAnalyzer.BackgroundJobs.Infrastructure.Repositories;

public class BackgroundJobExecutionRepository : IBackgroundJobExecutionRepository
{
    private readonly BackgroundJobsDbContext _context;

    public BackgroundJobExecutionRepository(BackgroundJobsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(BackgroundJobExecution execution, CancellationToken cancellationToken = default)
    {
        await _context.BackgroundJobExecutions.AddAsync(execution, cancellationToken);
    }

    public void Update(BackgroundJobExecution execution)
    {
        _context.BackgroundJobExecutions.Update(execution);
    }
}