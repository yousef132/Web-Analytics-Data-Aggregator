using DataAggergator.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAggergator.Infrastructure.Configurations
{
    internal class DailyStatsConfigurations : IEntityTypeConfiguration<DailyStats>
    {
        public void Configure(EntityTypeBuilder<DailyStats> builder)
        {
            builder.ToTable("daily_stats");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                   .ValueGeneratedOnAdd();

            builder.Property(x => x.Date)
                   .HasColumnType("date")
                   .IsRequired();

            builder.HasIndex(x => x.Date)
                   .IsUnique();

            builder.Property(x => x.LastUpdatedAt)
                   .HasColumnType("timestamp with time zone")
                   .HasDefaultValueSql("now()");

            builder.Property(x => x.TotalUsers)
                   .IsRequired();

            builder.Property(x => x.TotalSessions)
                   .IsRequired();

            builder.Property(x => x.TotalViews)
                   .IsRequired();

            builder.Property(x => x.AvgPerformance)
                   .HasPrecision(5, 2)
                   .IsRequired();
        }
    }
}
