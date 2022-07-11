using System.Text.Json;

namespace Nucleus;

public record EventRecord(long Id, string AggregateId, long Sequence, string EventType, string EventData)
{
    public object Deserialize()
    {
        return JsonSerializer.Deserialize(EventData, EventRegistry.GetType(EventType))!;
    }
}