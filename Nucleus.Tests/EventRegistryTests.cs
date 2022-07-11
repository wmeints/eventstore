using Nucleus.Tests.Support;

namespace Nucleus.Tests;

public class EventRegistryTests
{
    public EventRegistryTests()
    {
        if (!EventRegistry.IsRegistered(typeof(MyEvent)))
        {
            EventRegistry.Register(typeof(MyEvent));    
        }
        
    }
    
    [Fact]
    public void CanGetSchemaNameForType()
    {
        Assert.Equal("Nucleus.Tests.Support.MyEvent", EventRegistry.GetSchemaName(typeof(MyEvent)));
    }

    [Fact]
    public void CanGetTypeForSchemaName()
    {
        Assert.Equal(typeof(MyEvent), EventRegistry.GetType("Nucleus.Tests.Support.MyEvent"));
    }

    [Fact]
    public void CanCheckForRegisteredSchemaName()
    {
        Assert.True(EventRegistry.IsRegistered("Nucleus.Tests.Support.MyEvent"));
    }

    [Fact]
    public void CanCheckForRegisteredType()
    {
        Assert.True(EventRegistry.IsRegistered(typeof(MyEvent)));
    }
}