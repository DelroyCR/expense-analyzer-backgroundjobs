using ExpenseAnalyzer.BackgroundJobs.Application.DTOs;
using ExpenseAnalyzer.BackgroundJobs.Application.Services;
using ExpenseAnalyzer.BackgroundJobs.Api.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ExpenseAnalyzer.BackgroundJobs.Api.BackgroundServices;

public sealed class DailySummaryScheduler : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DailySummaryScheduler> _logger;
    private readonly SummarySchedulerOptions _options;

    public DailySummaryScheduler(
        IServiceScopeFactory scopeFactory,
        ILogger<DailySummaryScheduler> logger,
        IOptions<SummarySchedulerOptions> options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("DailySummaryScheduler deshabilitado por configuración.");
            return;
        }

        _logger.LogInformation(
            "DailySummaryScheduler iniciado. RunOnStartup={RunOnStartup}, IntervalSeconds={IntervalSeconds}, ProcessDateOffsetDays={ProcessDateOffsetDays}",
            _options.RunOnStartup,
            _options.IntervalSeconds,
            _options.ProcessDateOffsetDays);

        if (_options.RunOnStartup)
        {
            await RunOnceSafelyAsync(stoppingToken);
        }

        var intervalSeconds = Math.Max(1, _options.IntervalSeconds);
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(intervalSeconds));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunOnceSafelyAsync(stoppingToken);
        }
    }

    private async Task RunOnceSafelyAsync(CancellationToken stoppingToken)
    {
        try
        {
            await RunOnceAsync(stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("DailySummaryScheduler cancelado.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ejecutando DailySummaryScheduler.");
        }
    }

    private async Task RunOnceAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var service = scope.ServiceProvider.GetRequiredService<IGenerateSummarySnapshotsService>();

        var processDate = DateTime.UtcNow.Date.AddDays(_options.ProcessDateOffsetDays);

        var request = new GenerateDailySummariesRequestDto
        {
            DateUtc = processDate
        };

        var result = await service.GenerateDailyAsync(
            request,
            triggeredBy: "Scheduler",
            cancellationToken: stoppingToken);

        _logger.LogInformation(
            "Scheduler ejecutado. DateUtc={DateUtc}, Status={Status}, ProcessedUsers={ProcessedUsers}, GeneratedSnapshots={GeneratedSnapshots}",
            request.DateUtc,
            result.Status,
            result.ProcessedUsers,
            result.GeneratedSnapshots);
    }
}