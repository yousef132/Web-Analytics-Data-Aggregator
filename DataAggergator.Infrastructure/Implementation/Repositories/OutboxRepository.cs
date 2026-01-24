using System.Text.Json;
using DataAggergator.Application.Abstractions.Reposioties;
using DataAggergator.Domain.Models;
using Serilog;

namespace DataAggergator.Infrastructure.Implementation.Repositories
{
    internal class OutboxRepository : IOutboxRepository
    {
        private readonly AnalyticsDbContext _context;
        public OutboxRepository(AnalyticsDbContext context)
        {
            _context = context;
        }
        public async Task AddToOutbox<T>(T message) where T : class
        {
            try
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                var outboxMessage = new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    OccurredOnUtc = DateTime.UtcNow,
                    Type = typeof(T).AssemblyQualifiedName!,
                    Content = JsonSerializer.Serialize(message)
                };

                await _context.Set<T>().AddAsync(message);

                await _context.Set<OutboxMessage>().AddAsync(outboxMessage);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to add message to outbox");
                throw; 
            }
        }
    }

}
