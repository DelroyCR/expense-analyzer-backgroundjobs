using ExpenseAnalyzer.BackgroundJobs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseAnalyzer.BackgroundJobs.Infrastructure.Persistence.Configurations;

public class BackgroundJobExecutionConfiguration : IEntityTypeConfiguration<BackgroundJobExecution>
{
    public void Configure(EntityTypeBuilder<BackgroundJobExecution> builder)
    {
        builder.ToTable("BackgroundJobExecutions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.JobName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.TriggeredBy)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(2000);
    }
}