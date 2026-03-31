using ExpenseAnalyzer.BackgroundJobs.Application.Abstractions;
using ExpenseAnalyzer.BackgroundJobs.Application.DTOs;
using ExpenseAnalyzer.BackgroundJobs.Application.Services;
using ExpenseAnalyzer.BackgroundJobs.Domain.Entities;
using Moq;
using Xunit;

namespace ExpenseAnalyzer.BackgroundJobs.UnitTests.Services;

public class GenerateSummarySnapshotsServiceTests
{
    [Fact]
    public async Task GenerateDailyAsync_WhenTransactionsExistAndSnapshotDoesNotExist_CreatesSnapshot_LogsExecution_AndReturnsSucceeded()
    {
        var userId = Guid.NewGuid();
        var dateUtc = new DateTime(2026, 03, 30, 15, 45, 00, DateTimeKind.Utc);

        var transactions = new List<SourceTransaction>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Date = dateUtc.AddHours(-2),
                Amount = 250m
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Date = dateUtc.AddHours(-1),
                Amount = -100m
            }
        };

        var sourceTransactionRepositoryMock = new Mock<ISourceTransactionRepository>();
        var summarySnapshotRepositoryMock = new Mock<ISummarySnapshotRepository>();
        var backgroundJobExecutionRepositoryMock = new Mock<IBackgroundJobExecutionRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        SummarySnapshot? addedSnapshot = null;
        BackgroundJobExecution? addedExecution = null;
        BackgroundJobExecution? updatedExecution = null;

        sourceTransactionRepositoryMock
            .Setup(x => x.GetByDateRangeAsync(
                dateUtc.Date,
                dateUtc.Date.AddDays(1),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions);

        summarySnapshotRepositoryMock
            .Setup(x => x.GetByUserAndPeriodAsync(
                userId,
                dateUtc.Date,
                dateUtc.Date.AddDays(1),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((SummarySnapshot?)null);

        summarySnapshotRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<SummarySnapshot>(), It.IsAny<CancellationToken>()))
            .Callback<SummarySnapshot, CancellationToken>((snapshot, _) => addedSnapshot = snapshot)
            .Returns(Task.CompletedTask);

        backgroundJobExecutionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<BackgroundJobExecution>(), It.IsAny<CancellationToken>()))
            .Callback<BackgroundJobExecution, CancellationToken>((execution, _) => addedExecution = execution)
            .Returns(Task.CompletedTask);

        backgroundJobExecutionRepositoryMock
            .Setup(x => x.Update(It.IsAny<BackgroundJobExecution>()))
            .Callback<BackgroundJobExecution>(execution => updatedExecution = execution);

        unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = new GenerateSummarySnapshotsService(
            sourceTransactionRepositoryMock.Object,
            summarySnapshotRepositoryMock.Object,
            backgroundJobExecutionRepositoryMock.Object,
            unitOfWorkMock.Object);

        var request = new GenerateDailySummariesRequestDto
        {
            DateUtc = dateUtc,
            UserId = null
        };

        var result = await service.GenerateDailyAsync(
            request,
            triggeredBy: "Manual",
            cancellationToken: CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.ExecutionId);
        Assert.Equal("GenerateDailySummarySnapshots", result.JobName);
        Assert.Equal("Succeeded", result.Status);
        Assert.Equal(1, result.ProcessedUsers);
        Assert.Equal(1, result.GeneratedSnapshots);
        Assert.Equal(dateUtc.Date, result.DateUtc);

        Assert.NotNull(addedSnapshot);
        Assert.Equal(userId, addedSnapshot!.UserId);
        Assert.Equal(dateUtc.Date, addedSnapshot.PeriodStartUtc);
        Assert.Equal(dateUtc.Date.AddDays(1), addedSnapshot.PeriodEndUtc);
        Assert.Equal(250m, addedSnapshot.IncomeTotal);
        Assert.Equal(100m, addedSnapshot.ExpenseTotal);
        Assert.Equal(150m, addedSnapshot.NetTotal);
        Assert.Equal(2, addedSnapshot.TransactionCount);

        Assert.NotNull(addedExecution);
        Assert.Equal("GenerateDailySummarySnapshots", addedExecution!.JobName);
        Assert.Equal("Manual", addedExecution.TriggeredBy);
        Assert.Equal(1, addedExecution.AttemptCount);

        Assert.NotNull(updatedExecution);
        Assert.Equal("Succeeded", updatedExecution!.Status);
        Assert.NotNull(updatedExecution.FinishedAtUtc);
        Assert.Null(updatedExecution.ErrorMessage);

        sourceTransactionRepositoryMock.Verify(x => x.GetByDateRangeAsync(
            dateUtc.Date,
            dateUtc.Date.AddDays(1),
            null,
            It.IsAny<CancellationToken>()), Times.Once);

        summarySnapshotRepositoryMock.Verify(x => x.GetByUserAndPeriodAsync(
            userId,
            dateUtc.Date,
            dateUtc.Date.AddDays(1),
            It.IsAny<CancellationToken>()), Times.Once);

        summarySnapshotRepositoryMock.Verify(x => x.AddAsync(
            It.IsAny<SummarySnapshot>(),
            It.IsAny<CancellationToken>()), Times.Once);

        summarySnapshotRepositoryMock.Verify(x => x.Update(It.IsAny<SummarySnapshot>()), Times.Never);

        backgroundJobExecutionRepositoryMock.Verify(x => x.AddAsync(
            It.IsAny<BackgroundJobExecution>(),
            It.IsAny<CancellationToken>()), Times.Once);

        backgroundJobExecutionRepositoryMock.Verify(x => x.Update(
            It.IsAny<BackgroundJobExecution>()), Times.Once);

        unitOfWorkMock.Verify(x => x.SaveChangesAsync(
            It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}