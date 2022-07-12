using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Nucleus;

internal class SaveSnapshotStoreOperation<TContext>: EventStoreOperation<TContext> where TContext : DbContext
{
    private readonly string _aggregateId;
    private readonly object _snapshot;
    private readonly long _expectedVersion;

    public SaveSnapshotStoreOperation(string aggregateId, long expectedVersion, object snapshot)
    {
        _aggregateId = aggregateId;
        _snapshot = snapshot;
        _expectedVersion = expectedVersion;
    }

    public override Task ExecuteAsync(EventStoreOperationContext<TContext> context)
    {
        var serializedData = JsonSerializer.Serialize(_snapshot);
        
        context.Snapshots.Add(new SnapshotRecord(0L, _aggregateId, _expectedVersion,
            context.EventStoreSchemaRegistry.GetSchemaName(_snapshot.GetType()), serializedData));

        return Task.CompletedTask;
    }

    public override IEnumerable<object> Events => Enumerable.Empty<object>();
}