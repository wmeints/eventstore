using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Nucleus;

public class EventStore<TContext> : IEventStore where TContext : DbContext
{
    private List<EventStoreOperation<TContext>> Operations { get; } = new();
    private TContext Context { get; set; }
    private DbSet<EventRecord> Events => Context.Set<EventRecord>();

    public EventStore(TContext context)
    {
        Context = context;
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
        foreach(var operation in Operations)
        {
            await operation.ExecuteAsync(Context);
        }

        var affectedRows = await Context.SaveChangesAsync();
        
        Operations.Clear();

        return affectedRows;
    }
}