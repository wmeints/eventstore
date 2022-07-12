namespace Nucleus;

public class AggregateFactory
{
    public static TAggregate Create<TAggregate, TId>(TId id, long version, IEnumerable<object> events)
    {
        return (TAggregate)Activator.CreateInstance(typeof(TAggregate), false, id, version, events)!;
    }

    public static TAggregate Create<TAggregate, TId>(TId id, long version, object snapshot, IEnumerable<object> events)
    {
        return (TAggregate)Activator.CreateInstance(typeof(TAggregate), false, id, version, snapshot, events)!;
    }
}