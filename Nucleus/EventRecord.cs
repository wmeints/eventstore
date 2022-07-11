using System.Text.Json;

namespace Nucleus;

public record EventRecord(long Id, string AggregateId, long Sequence, string EventType, string EventData)
{
    public object Deserialize(EventRegistry eventRegistry)
    {
        return JsonSerializer.Deserialize(EventData, eventRegistry.GetType(EventType))!;
    }
}