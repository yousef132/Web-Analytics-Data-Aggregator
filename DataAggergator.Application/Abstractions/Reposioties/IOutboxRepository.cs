namespace DataAggergator.Application.Abstractions.Reposioties
{
    public interface IOutboxRepository
    {
        Task AddToOutbox<T>(T outboxMessage) where T : class;
    }

}
