using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Nucleus.Projections;
using Nucleus.StoreOperations;

namespace Nucleus;

public class EventStore<TContext> : IEventStore where TContext : DbContext
{
    private readonly List<EventStoreOperation<TContext>> _operations = new();
    private readonly TContext _dbContext;
    private readonly EventRegistry _eventRegistry;
    private readonly ProjectionEngine<TContext> _projectionEngine;

    public EventStore(TContext dbContext, ProjectionEngine<TContext> projectionEngineEngine,
        EventRegistry eventRegistry)
    {
        _dbContext = dbContext;
        _eventRegistry = eventRegistry;
        _projectionEngine = projectionEngineEngine;
    }

    public async Task<TAggregate?> GetAsync<TAggregate, TIdentity>(TIdentity id) where TAggregate : class
    {
        ArgumentNullException.ThrowIfNull(id);

        var eventRecords = await _dbContext.Set<EventRecord>()
            .Where(x => x.AggregateId == id.ToString()!)
            .OrderBy(x => x.Sequence)
            .ToListAsync();

        if (eventRecords.Count == 0)
        {
            return default;
        }

        var events = (IEnumerable<object>)eventRecords.Select(x => x.Deserialize(_eventRegistry)).ToList();
        var currentVersion = eventRecords.Max(x => x.Sequence);

        AggregateVersionCache.Put(id.ToString()!, currentVersion);

        return (TAggregate)Activator.CreateInstance(typeof(TAggregate), false, id, currentVersion, events)!;
    }

    public void CreateStream<TIdentity>(TIdentity id, IEnumerable<object> events)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(events);

        _operations.Add(new AppendEventsOperation<TContext>(id.ToString()!, 0L, events));
    }

    public void AppendToStream<TIdentity>(TIdentity id, long expectedVersion, IEnumerable<object> events)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(events);

        _operations.Add(new AppendEventsOperation<TContext>(id.ToString()!, expectedVersion, events));
    }

    public async Task<int> SaveChangesAsync()
    {
        using var transactionScope = new TransactionScope();

        var trackedEvents = new List<object>();
        var operationContext = new EventStoreOperationContext<TContext>(_eventRegistry, _dbContext);

        // Always execute the event store operations first before running the projections.
        // If the projectors want to use events, they can grab them from the DbContext.
        foreach (var operation in _operations)
        {
            await operation.ExecuteAsync(operationContext);
            trackedEvents.AddRange(operation.Events);
        }

        await _projectionEngine.RunAsync(trackedEvents);

        var affectedRows = await _dbContext.SaveChangesAsync();

        transactionScope.Complete();
        _operations.Clear();

        return affectedRows;
    }
}