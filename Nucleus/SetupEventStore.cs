namespace Nucleus;

public class SetupEventStore
{
    public static SetupEventStore Instance { get; } = new();
    
    public void RegisterEvent<T>(string? schemaName = null)
    {
        if (string.IsNullOrEmpty(schemaName))
        {
            EventRegistry.Register(typeof(T));    
        }
        else
        {
            EventRegistry.Register(typeof(T), schemaName);
        }
    }
}