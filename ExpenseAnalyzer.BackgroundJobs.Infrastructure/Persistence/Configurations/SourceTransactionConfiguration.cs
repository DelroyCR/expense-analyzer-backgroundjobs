using ExpenseAnalyzer.BackgroundJobs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseAnalyzer.BackgroundJobs.Infrastructure.Persistence.Configurations;

public class SourceTransactionConfiguration : IEntityTypeConfiguration<SourceTransaction>
{
    public void Configure(EntityTypeBuilder<SourceTransaction> builder)
    {
        builder.ToTable("Transactions", t => t.ExcludeFromMigrations());

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);
    }
}