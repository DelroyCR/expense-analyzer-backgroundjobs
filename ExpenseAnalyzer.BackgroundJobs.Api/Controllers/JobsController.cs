using ExpenseAnalyzer.BackgroundJobs.Application.DTOs;
using ExpenseAnalyzer.BackgroundJobs.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseAnalyzer.BackgroundJobs.Api.Controllers;

[ApiController]
[Route("api/jobs")]
public class JobsController : ControllerBase
{
    private readonly IGenerateSummarySnapshotsService _generateSummarySnapshotsService;

    public JobsController(IGenerateSummarySnapshotsService generateSummarySnapshotsService)
    {
        _generateSummarySnapshotsService = generateSummarySnapshotsService;
    }

 [HttpPost("generate-daily-summaries")]
    public async Task<ActionResult<GenerateDailySummariesResultDto>> GenerateDailySummaries(
        [FromBody] GenerateDailySummariesRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest("Request body is required.");
        }

        if (request.DateUtc == default)
        {
            return BadRequest("DateUtc is required.");
        }

        var result = await _generateSummarySnapshotsService.GenerateDailyAsync(
            request,
            cancellationToken: cancellationToken);

        return Ok(result);
    }
}