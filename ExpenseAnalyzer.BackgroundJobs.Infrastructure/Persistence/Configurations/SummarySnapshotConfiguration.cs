using ExpenseAnalyzer.BackgroundJobs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseAnalyzer.BackgroundJobs.Infrastructure.Persistence.Configurations;

public class SummarySnapshotConfiguration : IEntityTypeConfiguration<SummarySnapshot>
{
    public void Configure(EntityTypeBuilder<SummarySnapshot> builder)
    {
        builder.ToTable("SummarySnapshots");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.IncomeTotal).HasPrecision(18, 2);
        builder.Property(x => x.ExpenseTotal).HasPrecision(18, 2);
        builder.Property(x => x.NetTotal).HasPrecision(18, 2);

        builder.HasIndex(x => new { x.UserId, x.PeriodStartUtc, x.PeriodEndUtc })
               .IsUnique();
    }
}