using Nucleus.Tests.Support;

namespace Nucleus.Tests;

public class EventStoreSchemaRegistryTests
{
    private readonly EventStoreSchemaRegistry _eventStoreSchemaRegistry;

    public EventStoreSchemaRegistryTests()
    {
        _eventStoreSchemaRegistry = new EventStoreSchemaRegistry();
        _eventStoreSchemaRegistry.Register(typeof(MyEvent));
    }

    [Fact]
    public void CanGetSchemaNameForType()
    {
        Assert.Equal("Nucleus.Tests.Support.MyEvent", 
            _eventStoreSchemaRegistry.GetSchemaName(typeof(MyEvent)));
    }

    [Fact]
    public void CanGetTypeForSchemaName()
    {
        Assert.Equal(typeof(MyEvent), 
            _eventStoreSchemaRegistry.GetType("Nucleus.Tests.Support.MyEvent"));
    }

    [Fact]
    public void CanCheckForRegisteredSchemaName()
    {
        Assert.True(_eventStoreSchemaRegistry.IsRegistered("Nucleus.Tests.Support.MyEvent"));
    }

    [Fact]
    public void CanCheckForRegisteredType()
    {
        Assert.True(_eventStoreSchemaRegistry.IsRegistered(typeof(MyEvent)));
    }
}