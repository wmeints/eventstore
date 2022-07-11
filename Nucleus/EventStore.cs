using System.Transactions;
using Microsoft.EntityFrameworkCore;

namespace Nucleus;

public class EventStore<TContext> : IEventStore where TContext : DbContext
{
    private List<EventStoreOperation<TContext>> Operations { get; } = new();
    private TContext Context { get; }
    private DbSet<EventRecord> Events => Context.Set<EventRecord>();

    private ProjectionEngine<TContext> Projections { get; }

    public EventStore(TContext context, ProjectionEngine<TContext> projectionEngine)
    {
        Context = context;
        Projections = projectionEngine;
    }

    public async Task<TAggregate?> GetAsync<TAggregate, TIdentity>(TIdentity id) where TAggregate : class
    {
        ArgumentNullException.ThrowIfNull(id);

        var eventRecords = await Events
            .Where(x => x.AggregateId == id.ToString()!)
            .OrderBy(x => x.Sequence)
            .ToListAsync();

        if (eventRecords.Count == 0)
        {
            return default(TAggregate);
        }

        var events = (IEnumerable<object>)eventRecords.Select(x => x.Deserialize()).ToList();
        var currentVersion = eventRecords.Max(x => x.Sequence);

        AggregateVersionCache.Put(id.ToString()!, currentVersion);

        return (TAggregate)Activator.CreateInstance(typeof(TAggregate), false, id, currentVersion, events)!;
    }

    public void CreateStream<TIdentity>(TIdentity id, IEnumerable<object> events)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(events);

        Operations.Add(new AppendEventsOperation<TContext>(id.ToString()!, 0L, events));
    }

    public void AppendStream<TIdentity>(TIdentity id, long expectedVersion, IEnumerable<object> events)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(events);

        Operations.Add(new AppendEventsOperation<TContext>(id.ToString()!, expectedVersion, events));
    }

    public async Task<int> SaveChangesAsync()
    {
        using var transactionScope = new TransactionScope();

        var trackedEvents = new List<object>();

        // Always execute the event store operations first before running the projections.
        // If the projectors want to use events, they can grab them from the DbContext.
        foreach (var operation in Operations)
        {
            await operation.ExecuteAsync(Context);
            trackedEvents.AddRange(operation.Events);
        }

        foreach (var evt in trackedEvents)
        {
            await Projections.RunAsync(trackedEvents);
        }

        var affectedRows = await Context.SaveChangesAsync();
        
        transactionScope.Complete();
        Operations.Clear();

        return affectedRows;
    }
}