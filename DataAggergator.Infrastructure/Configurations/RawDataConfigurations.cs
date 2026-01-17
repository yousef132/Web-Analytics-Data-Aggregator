using DataAggergator.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAggergator.Infrastructure.Configurations
{
    internal class RawDataConfigurations : IEntityTypeConfiguration<RawData>
    {
        public void Configure(EntityTypeBuilder<RawData> builder)
        {
            builder.ToTable("raw_data");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                   .ValueGeneratedOnAdd();

            builder.Property(x => x.Date)
                   .HasColumnType("date")
                   .IsRequired();

            builder.Property(x => x.Page)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(x => x.Users)
                   .IsRequired();

            builder.Property(x => x.Sessions)
                   .IsRequired();

            builder.Property(x => x.Views)
                   .IsRequired();

            builder.Property(x => x.PerformanceScore)
                   .HasPrecision(5, 2) 
                   .IsRequired();

            builder.Property(x => x.LCP_ms)
                   .HasPrecision(10, 2) 
                   .IsRequired();

            builder.Property(x => x.ReceivedAt)
                   .HasColumnType("timestamp with time zone")
                   .HasDefaultValueSql("now()");

            builder.HasIndex(x => new { x.Date, x.Page });
        }
    }

}
