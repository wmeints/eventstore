namespace Nucleus;

public interface IEventStore
{
    Task<TAggregate?> GetAsync<TAggregate, TIdentity>(TIdentity id) where TAggregate : class;
    void CreateStream<TIdentity>(TIdentity id, IEnumerable<object> events);
    void AppendToStream<TIdentity>(TIdentity id, long expectedVersion, IEnumerable<object> events);
    Task<int> SaveChangesAsync();
}