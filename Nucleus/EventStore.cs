using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Nucleus;

public class EventStore<TContext> : IEventStore where TContext : DbContext
{
    private TContext Context { get; set; }
    private DbSet<EventRecord> Events => Context.Set<EventRecord>();

    public EventStore(TContext context)
    {
        Context = context;
    }

    public async Task<TAggregate?> GetAsync<TAggregate, TIdentity>(TIdentity id) where TAggregate : class
    {
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

        return (TAggregate)Activator.CreateInstance(typeof(TAggregate), false, id, currentVersion, events)!;
    }

    public async Task CreateStreamAsync<TIdentity>(TIdentity id, IEnumerable<object> events)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(events);

        var version = 0L;

        var existingRecords = await Events.AnyAsync(x => x.AggregateId == id.ToString());

        if (existingRecords)
        {
            throw new InvalidOperationException(
                "There's already a stream for this aggregate. Please use AppendAsync instead.");
        }

        await AppendEventsInternal(id, version, events);
    }

    public async Task AppendAsync<TIdentity>(TIdentity id, long expectedVersion, IEnumerable<object> events)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(events);

        var eventCount = await Events.Where(x => x.AggregateId == id.ToString()).CountAsync();
        var currentVersion = 0L;

        if (eventCount > 0)
        {
            currentVersion = await Events
                .Where(x => x.AggregateId == id.ToString())
                .MaxAsync(x => x.Sequence);
        }

        if (currentVersion != expectedVersion)
        {
            throw new DBConcurrencyException("Event stream has been modified. Reload the stream and try again.");
        }

        await AppendEventsInternal(id, expectedVersion, events);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await Context.SaveChangesAsync();
    }

    private async Task AppendEventsInternal<TIdentity>(
        [DisallowNull] TIdentity id, long version, IEnumerable<object> events)
    {
        var eventRecords = new List<EventRecord>();

        foreach (var evt in events)
        {
            var serializedData = JsonSerializer.Serialize(evt);

            eventRecords.Add(new EventRecord(
                0L,
                id.ToString()!,
                ++version,
                EventRegistry.GetSchemaName(evt.GetType()),
                serializedData));
        }

        await Events.AddRangeAsync(eventRecords);
    }
}