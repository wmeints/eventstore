using System.Text.Json;

namespace FizzyLogic.EventStore;

public record EventRecord(long Id, string AggregateId, long Sequence, string EventType, string EventData)
{
    public object Deserialize(EventStoreSchemaRegistry eventStoreSchemaRegistry)
    {
        return JsonSerializer.Deserialize(EventData, eventStoreSchemaRegistry.GetType(EventType))!;
    }
}