using System.Transactions;
using Microsoft.EntityFrameworkCore;

namespace FizzyLogic.EventStore;

public class EventStore<TContext> : IEventStore where TContext : DbContext
{
    private readonly List<EventStoreOperation<TContext>> _operations = new();
    private readonly TContext _dbContext;
    private readonly EventStoreSchemaRegistry _eventStoreSchemaRegistry;
    private readonly ProjectionEngine<TContext> _projectionEngine;

    public EventStore(TContext dbContext, ProjectionEngine<TContext> projectionEngineEngine,
        EventStoreSchemaRegistry eventStoreSchemaRegistry)
    {
        _dbContext = dbContext;
        _eventStoreSchemaRegistry = eventStoreSchemaRegistry;
        _projectionEngine = projectionEngineEngine;
    }

    public async Task<TAggregate?> GetAsync<TAggregate, TIdentity>(TIdentity id) where TAggregate : class
    {
        ArgumentNullException.ThrowIfNull(id);

        var startFromVersion = -1L;

        var isTombStoned = await _dbContext.Set<EventRecord>()
            .AnyAsync(x => x.AggregateId == id.ToString() && x.EventType == "Tombstone");

        // Tomb stoned streams aren't available and we shouldn't return them when asked for an aggregate.
        
        if (isTombStoned)
        {
            return default;
        }
        
        var snapshotRecord = await _dbContext.Set<SnapshotRecord>()
            .Where(x => x.AggregateId == id.ToString())
            .OrderByDescending(x => x.Sequence)
            .FirstOrDefaultAsync();

        if (snapshotRecord != null)
        {
            startFromVersion = snapshotRecord.Sequence;
        }

        var eventRecords = await _dbContext.Set<EventRecord>()
            .Where(x => x.AggregateId == id.ToString()!)
            .Where(x => x.Sequence > startFromVersion)
            .OrderBy(x => x.Sequence)
            .ToListAsync();

        if (eventRecords.Count == 0)
        {
            return default;
        }

        var events = (IEnumerable<object>) eventRecords.Select(x => x.Deserialize(_eventStoreSchemaRegistry)).ToList();
        var currentVersion = eventRecords.Max(x => x.Sequence);

        AggregateVersionCache.Put(id.ToString()!, currentVersion);

        return snapshotRecord switch
        {
            { } => AggregateFactory.Create<TAggregate, TIdentity>(id, currentVersion,
                snapshotRecord.Deserialize(_eventStoreSchemaRegistry), events),
            _ => AggregateFactory.Create<TAggregate, TIdentity>(id, currentVersion, events)
        };
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

    public void SaveSnapshot<TIdentity, TSnapshot>(TIdentity id, long version, TSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(snapshot);

        _operations.Add(new SaveSnapshotStoreOperation<TContext>(id.ToString()!, version, snapshot));
    }

    public void Delete<TIdentity>(TIdentity id, long version)
    {
        ArgumentNullException.ThrowIfNull(id);
        
        _operations.Add(new AppendEventsOperation<TContext>(id.ToString()!, version, new [] { Tombstone.Instance }));
    }

    public async Task<int> SaveChangesAsync()
    {
        using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var trackedEvents = new List<object>();
        var operationContext = new EventStoreOperationContext<TContext>(_eventStoreSchemaRegistry, _dbContext);

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