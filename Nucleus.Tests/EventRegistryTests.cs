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
        Assert.Equal("Nucleus.Tests.MyEvent", EventRegistry.GetSchemaName(typeof(MyEvent)));
    }

    [Fact]
    public void CanGetTypeForSchemaName()
    {
        Assert.Equal(typeof(MyEvent), EventRegistry.GetType("Nucleus.Tests.MyEvent"));
    }

    [Fact]
    public void CanCheckForRegisteredSchemaName()
    {
        Assert.True(EventRegistry.IsRegistered("Nucleus.Tests.MyEvent"));
    }

    [Fact]
    public void CanCheckForRegisteredType()
    {
        Assert.True(EventRegistry.IsRegistered(typeof(MyEvent)));
    }
}