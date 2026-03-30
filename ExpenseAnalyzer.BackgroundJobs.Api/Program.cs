using Microsoft.EntityFrameworkCore;
using ExpenseAnalyzer.BackgroundJobs.Application.Abstractions;
using ExpenseAnalyzer.BackgroundJobs.Application.Services;
using ExpenseAnalyzer.BackgroundJobs.Infrastructure.Persistence;
using ExpenseAnalyzer.BackgroundJobs.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<BackgroundJobsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ISourceTransactionRepository, SourceTransactionRepository>();
builder.Services.AddScoped<ISummarySnapshotRepository, SummarySnapshotRepository>();
builder.Services.AddScoped<IBackgroundJobExecutionRepository, BackgroundJobExecutionRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IGenerateSummarySnapshotsService, GenerateSummarySnapshotsService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();