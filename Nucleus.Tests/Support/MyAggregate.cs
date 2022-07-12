namespace Nucleus.Tests.Support;

public class MyAggregate
{
    public long Version { get; set; }
    public Guid Id { get; private set; }
    public string Message { get; set; } = "";
    public List<object> Events { get; set; }
    
    protected MyAggregate(Guid id, long version, IEnumerable<object> events)
    {
        Id = id;
        Version = version;
        Events = events.ToList();
    }

    protected MyAggregate(Guid id, long version, object snapshot, IEnumerable<object> events)
    {
        Id = id;
        Version = version;
        Events = events.ToList();

        if (snapshot is MySnapshot mySnapshotInstance)
        {
            Message = mySnapshotInstance.Message;
        }
    }
}