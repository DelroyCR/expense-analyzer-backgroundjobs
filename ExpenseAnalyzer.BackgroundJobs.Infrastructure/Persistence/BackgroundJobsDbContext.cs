using ExpenseAnalyzer.BackgroundJobs.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseAnalyzer.BackgroundJobs.Infrastructure.Persistence;

public class BackgroundJobsDbContext : DbContext
{
    public BackgroundJobsDbContext(DbContextOptions<BackgroundJobsDbContext> options)
        : base(options)
    {
    }

    public DbSet<SummarySnapshot> SummarySnapshots => Set<SummarySnapshot>();
    public DbSet<BackgroundJobExecution> BackgroundJobExecutions => Set<BackgroundJobExecution>();
    public DbSet<SourceTransaction> SourceTransactions => Set<SourceTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BackgroundJobsDbContext).Assembly);
    }
}