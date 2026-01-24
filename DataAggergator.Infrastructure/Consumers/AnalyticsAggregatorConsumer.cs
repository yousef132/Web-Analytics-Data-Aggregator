using DataAggergator.Application.Dtos;
using DataAggergator.Infrastructure.Messages;
using DataAggergator.Domain.Models;
using MassTransit;
using Serilog;
using Serilog.Context;

namespace DataAggergator.Infrastructure.Consumers
{
    internal class AnalyticsAggregatorConsumer : IConsumer<AnalyticsRecordsAggregated>
    {
        private readonly AnalyticsDbContext _db;

        public AnalyticsAggregatorConsumer(AnalyticsDbContext db)
        {
            _db = db;
        }

        public async Task Consume(ConsumeContext<AnalyticsRecordsAggregated> context)
        {
            var retryAttempt = context.GetRetryAttempt();
            var correlationId = context.CorrelationId?.ToString() ?? Guid.NewGuid().ToString();
            using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
            using (Serilog.Context.LogContext.PushProperty("MessageType", nameof(AnalyticsRecordsAggregated)))

            {
                Log.Information(
                          "Consuming AnalyticsRecordsAggregated with {RecordCount} records, RetryAttempt={RetryAttempt}",
                          context.Message.Records.Count,
                          retryAttempt);

                try
                {
                    var records = context.Message.Records;

                    var dailyStats = BuildDailyStats(records);
                    var rawData = BuildRawData(records);

                    await using var transaction =
                        await _db.Database.BeginTransactionAsync(context.CancellationToken);

                    await _db.DailyStats.AddRangeAsync(dailyStats, context.CancellationToken);
                    await _db.RawData.AddRangeAsync(rawData, context.CancellationToken);

                    await _db.SaveChangesAsync(context.CancellationToken);
                    await transaction.CommitAsync(context.CancellationToken);

                    Log.Information("Successfully processed {RecordCount} records",
                        context.Message.Records.Count);
                }
                catch (Exception ex)
                {
                    // Serilog automatically logs exception + stacktrace
                    Log.Error(ex, "Error while saving AnalyticsRecordsAggregated message");

                    throw; // will trigger retry in MassTransit
                }
            }
        }

        private static List<DailyStats> BuildDailyStats(IEnumerable<AnalyticsRecord> records)
        {
            return records
                .GroupBy(r => r.Date)
                .Select(g =>
                {
                    var count = g.Count();

                    return new DailyStats
                    {
                        Date = g.Key,
                        TotalUsers = g.Sum(x => x.Users),
                        TotalSessions = g.Sum(x => x.Sessions),
                        TotalViews = g.Sum(x => x.Views),
                        AvgPerformance = count == 0 ? 0 : g.Average(x => x.PerformanceScore),
                    };
                })
                .ToList();
        }

        private static List<RawData> BuildRawData(IEnumerable<AnalyticsRecord> records)
        {
            return records.Select(r => new RawData
            {
                Date = r.Date,
                Page = r.Page,
                Users = r.Users,
                Sessions = r.Sessions,
                Views = r.Views,
                PerformanceScore = r.PerformanceScore,
                LCP_ms = r.LCP_ms,
            }).ToList();
        }
    }
}
