using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAggergator.Domain.Models;
using DataAggergator.Infrastructure.Sagas;
using Microsoft.EntityFrameworkCore;

namespace DataAggergator.Infrastructure
{
    public class AnalyticsDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<RawData> RawData => Set<RawData>();
        public DbSet<DailyStats> DailyStats => Set<DailyStats>();
        public DbSet<NewsLetterOnBoardingSagaData> SagaData { get; set; }
        public DbSet<OutboxMessage> OutboxMessages { get; set; }
        public DbSet<Order> Orders { get; set; }


        public AnalyticsDbContext()
        {
            
        }
        public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<NewsLetterOnBoardingSagaData>().HasKey(s => s.CorrelationId); //pk


            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AnalyticsDbContext).Assembly);
        }

        public DbSet<Subscriber> Subscribers { get; set; }

    }
}
