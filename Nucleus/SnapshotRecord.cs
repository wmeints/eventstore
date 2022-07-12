using System.Text.Json;

namespace Nucleus;

public record SnapshotRecord(long Id, string AggregateId, long Sequence, string SnapshotType, string SnapshotData)
{
    public object Deserialize(EventStoreSchemaRegistry eventStoreSchemaRegistry)
    {
        return JsonSerializer.Deserialize(SnapshotData, eventStoreSchemaRegistry.GetType(SnapshotType))!;
    }
}
