using Nucleus.Tests.Support;

namespace Nucleus.Tests;

public class EventRegistryTests
{
    private readonly EventRegistry _eventRegistry;
    
    public EventRegistryTests()
    {
        _eventRegistry = new EventRegistry();
        _eventRegistry.Register(typeof(MyEvent));
    }
    
    [Fact]
    public void CanGetSchemaNameForType()
    {
        Assert.Equal("Nucleus.Tests.Support.MyEvent", _eventRegistry.GetSchemaName(typeof(MyEvent)));
    }

    [Fact]
    public void CanGetTypeForSchemaName()
    {
        Assert.Equal(typeof(MyEvent), _eventRegistry.GetType("Nucleus.Tests.Support.MyEvent"));
    }

    [Fact]
    public void CanCheckForRegisteredSchemaName()
    {
        Assert.True(_eventRegistry.IsRegistered("Nucleus.Tests.Support.MyEvent"));
    }

    [Fact]
    public void CanCheckForRegisteredType()
    {
        Assert.True(_eventRegistry.IsRegistered(typeof(MyEvent)));
    }
}