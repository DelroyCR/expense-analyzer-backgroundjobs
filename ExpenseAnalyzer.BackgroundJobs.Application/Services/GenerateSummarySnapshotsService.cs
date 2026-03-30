using ExpenseAnalyzer.BackgroundJobs.Application.Abstractions;
using ExpenseAnalyzer.BackgroundJobs.Application.DTOs;
using ExpenseAnalyzer.BackgroundJobs.Domain.Entities;

namespace ExpenseAnalyzer.BackgroundJobs.Application.Services;

public class GenerateSummarySnapshotsService : IGenerateSummarySnapshotsService
{
    private readonly ISourceTransactionRepository _sourceTransactionRepository;
    private readonly ISummarySnapshotRepository _summarySnapshotRepository;
    private readonly IBackgroundJobExecutionRepository _backgroundJobExecutionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GenerateSummarySnapshotsService(
        ISourceTransactionRepository sourceTransactionRepository,
        ISummarySnapshotRepository summarySnapshotRepository,
        IBackgroundJobExecutionRepository backgroundJobExecutionRepository,
        IUnitOfWork unitOfWork)
    {
        _sourceTransactionRepository = sourceTransactionRepository;
        _summarySnapshotRepository = summarySnapshotRepository;
        _backgroundJobExecutionRepository = backgroundJobExecutionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<GenerateDailySummariesResultDto> GenerateDailyAsync(
        GenerateDailySummariesRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var execution = new BackgroundJobExecution
        {
            Id = Guid.NewGuid(),
            JobName = "GenerateDailySummarySnapshots",
            Status = "Running",
            StartedAtUtc = DateTime.UtcNow,
            AttemptCount = 1,
            TriggeredBy = "Manual"
        };

        await _backgroundJobExecutionRepository.AddAsync(execution, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            var periodStartUtc = request.DateUtc.Date;
            var periodEndUtc = periodStartUtc.AddDays(1);

            var transactions = await _sourceTransactionRepository.GetByDateRangeAsync(
                periodStartUtc,
                periodEndUtc,
                request.UserId,
                cancellationToken);

            var groupedByUser = transactions
                .GroupBy(t => t.UserId)
                .ToList();

            var processedUsers = 0;
            var generatedSnapshots = 0;

            foreach (var group in groupedByUser)
            {
                processedUsers++;

                var userId = group.Key;
                var incomeTotal = group.Where(x => x.Amount > 0).Sum(x => x.Amount);
                var expenseTotal = Math.Abs(group.Where(x => x.Amount < 0).Sum(x => x.Amount));
                var netTotal = group.Sum(x => x.Amount);
                var transactionCount = group.Count();

                var existingSnapshot = await _summarySnapshotRepository.GetByUserAndPeriodAsync(
                    userId,
                    periodStartUtc,
                    periodEndUtc,
                    cancellationToken);

                if (existingSnapshot is null)
                {
                    var newSnapshot = new SummarySnapshot
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        PeriodStartUtc = periodStartUtc,
                        PeriodEndUtc = periodEndUtc,
                        IncomeTotal = incomeTotal,
                        ExpenseTotal = expenseTotal,
                        NetTotal = netTotal,
                        TransactionCount = transactionCount,
                        GeneratedAtUtc = DateTime.UtcNow
                    };

                    await _summarySnapshotRepository.AddAsync(newSnapshot, cancellationToken);
                }
                else
                {
                    existingSnapshot.IncomeTotal = incomeTotal;
                    existingSnapshot.ExpenseTotal = expenseTotal;
                    existingSnapshot.NetTotal = netTotal;
                    existingSnapshot.TransactionCount = transactionCount;
                    existingSnapshot.GeneratedAtUtc = DateTime.UtcNow;

                    _summarySnapshotRepository.Update(existingSnapshot);
                }

                generatedSnapshots++;
            }

            execution.Status = "Succeeded";
            execution.FinishedAtUtc = DateTime.UtcNow;
            _backgroundJobExecutionRepository.Update(execution);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new GenerateDailySummariesResultDto
            {
                ExecutionId = execution.Id,
                JobName = execution.JobName,
                Status = execution.Status,
                ProcessedUsers = processedUsers,
                GeneratedSnapshots = generatedSnapshots,
                DateUtc = periodStartUtc
            };
        }
        catch (Exception ex)
        {
            execution.Status = "Failed";
            execution.FinishedAtUtc = DateTime.UtcNow;
            execution.ErrorMessage = ex.Message;

            _backgroundJobExecutionRepository.Update(execution);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            throw;
        }
    }
}