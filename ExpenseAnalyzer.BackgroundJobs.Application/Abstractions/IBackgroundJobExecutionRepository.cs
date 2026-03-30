using ExpenseAnalyzer.BackgroundJobs.Domain.Entities;

namespace ExpenseAnalyzer.BackgroundJobs.Application.Abstractions;

public interface IBackgroundJobExecutionRepository
{
    Task AddAsync(BackgroundJobExecution execution, CancellationToken cancellationToken = default);
    void Update(BackgroundJobExecution execution);
}