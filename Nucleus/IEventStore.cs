namespace Nucleus;

public interface IEventStore
{
    Task<TAggregate?> GetAsync<TAggregate, TIdentity>(TIdentity id) where TAggregate : class;
    Task CreateStreamAsync<TIdentity>(TIdentity id, IEnumerable<object> events);
    Task AppendAsync<TIdentity>(TIdentity id, long expectedVersion, IEnumerable<object> events);
    Task<int> SaveChangesAsync();
}