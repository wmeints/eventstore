using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Nucleus.StoreOperations;

internal class AppendEventsOperation<TContext> : EventStoreOperation<TContext> where TContext : DbContext
{
    private readonly string _aggregateId;
    private readonly long _expectedVersion;
    private readonly IEnumerable<object> _records;

    public override IEnumerable<object> Events => _records;

    public AppendEventsOperation(string aggregateId, long expectedVersion, IEnumerable<object> records)
    {
        _aggregateId = aggregateId;
        _expectedVersion = expectedVersion;
        _records = records;
    }

    public override async Task ExecuteAsync(EventStoreOperationContext<TContext> context)
    {
        var currentVersion = AggregateVersionCache.Get(_aggregateId);

        if (await context.Events.AnyAsync(e => e.AggregateId == _aggregateId))
        {
            currentVersion = context.Events
                .Where(x => x.AggregateId == _aggregateId)
                .Max(x => x.Sequence);
        }

        if (currentVersion != _expectedVersion)
        {
            throw new ConcurrencyException("Stream was already modified. Please reload the stream and try again.");
        }

        var eventRecords = new List<EventRecord>();

        foreach (var evt in _records)
        {
            var serializedEventData = JsonSerializer.Serialize(evt);
            
            eventRecords.Add(new EventRecord(0L, _aggregateId, ++currentVersion,
                context.EventStoreSchemaRegistry.GetSchemaName(evt.GetType()), serializedEventData));
        }

        await context.Events.AddRangeAsync(eventRecords);

        AggregateVersionCache.Put(_aggregateId, currentVersion);
    }
}