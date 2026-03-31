using Microsoft.EntityFrameworkCore;
using ExpenseAnalyzer.BackgroundJobs.Application.Abstractions;
using ExpenseAnalyzer.BackgroundJobs.Application.Services;
using ExpenseAnalyzer.BackgroundJobs.Infrastructure.Persistence;
using ExpenseAnalyzer.BackgroundJobs.Infrastructure.Repositories;
using ExpenseAnalyzer.BackgroundJobs.Api.BackgroundServices;
using ExpenseAnalyzer.BackgroundJobs.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<BackgroundJobsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ISourceTransactionRepository, SourceTransactionRepository>();
builder.Services.AddScoped<ISummarySnapshotRepository, SummarySnapshotRepository>();
builder.Services.AddScoped<IBackgroundJobExecutionRepository, BackgroundJobExecutionRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IGenerateSummarySnapshotsService, GenerateSummarySnapshotsService>();

builder.Services
    .AddOptions<SummarySchedulerOptions>()
    .Bind(builder.Configuration.GetSection(SummarySchedulerOptions.SectionName))
    .Validate(options => options.IntervalSeconds > 0, "SummaryScheduler:IntervalSeconds must be greater than 0.")
    .ValidateOnStart();
    
builder.Services.AddHostedService<DailySummaryScheduler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();