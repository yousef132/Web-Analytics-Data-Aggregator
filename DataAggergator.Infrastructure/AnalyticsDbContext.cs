using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAggergator.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAggergator.Infrastructure
{
    public class AnalyticsDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<RawData> RawData => Set<RawData>();
        public DbSet<DailyStats> DailyStats => Set<DailyStats>();
        public AnalyticsDbContext()
        {
            
        }
        public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AnalyticsDbContext).Assembly);
        }
    }
}
