namespace Nucleus;

public static class EventRegistry
{
    private static readonly Dictionary<string, Type> SchemaToType = new();
    private static readonly Dictionary<Type, string> TypeToSchema = new();

    public static void Register(Type type)
    {
        var schemaName = type.FullName!;
        Register(type, schemaName);
    }

    public static void Register(Type type, string schemaName)
    { 
        ArgumentNullException.ThrowIfNull(schemaName);
        ArgumentNullException.ThrowIfNull(type);

        if (TypeToSchema.ContainsKey(type))
        {
            throw new ArgumentException($"Type {type.FullName} is already registered");
        }
        
        SchemaToType.Add(schemaName, type);
        TypeToSchema.Add(type, schemaName);
    }

    public static bool IsRegistered(Type type)
    {
        return TypeToSchema.ContainsKey(type);
    }

    public static bool IsRegistered(string schemaName)
    {
        return SchemaToType.ContainsKey(schemaName);
    }

    public static string GetSchemaName(Type type)
    {
        if(!TypeToSchema.TryGetValue(type, out var schemaName))
        {
            throw new ArgumentException($"Type {type.FullName} is not registered");
        }

        return schemaName;
    }

    public static Type GetType(string schemaName)
    {
        if(!SchemaToType.TryGetValue(schemaName, out var type))
        {
            throw new ArgumentException($"Schema {schemaName} is not registered");
        }

        return type;
    }
}