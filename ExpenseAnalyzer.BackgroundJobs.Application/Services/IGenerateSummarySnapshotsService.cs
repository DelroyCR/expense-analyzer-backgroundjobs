using ExpenseAnalyzer.BackgroundJobs.Application.DTOs;

namespace ExpenseAnalyzer.BackgroundJobs.Application.Services;

public interface IGenerateSummarySnapshotsService
{
    Task<GenerateDailySummariesResultDto> GenerateDailyAsync(
        GenerateDailySummariesRequestDto request,
        CancellationToken cancellationToken = default);
}