using FizzyLogic.EventStore.Tests.Support;

namespace FizzyLogic.EventStore.Tests;

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
        Assert.Equal("FizzyLogic.EventStore.Tests.Support.MyEvent", 
            _eventStoreSchemaRegistry.GetSchemaName(typeof(MyEvent)));
    }

    [Fact]
    public void CanGetTypeForSchemaName()
    {
        Assert.Equal(typeof(MyEvent), 
            _eventStoreSchemaRegistry.GetType("FizzyLogic.EventStore.Tests.Support.MyEvent"));
    }

    [Fact]
    public void CanCheckForRegisteredSchemaName()
    {
        Assert.True(_eventStoreSchemaRegistry.IsRegistered("FizzyLogic.EventStore.Tests.Support.MyEvent"));
    }

    [Fact]
    public void CanCheckForRegisteredType()
    {
        Assert.True(_eventStoreSchemaRegistry.IsRegistered(typeof(MyEvent)));
    }
}