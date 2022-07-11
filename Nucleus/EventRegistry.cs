namespace Nucleus;

public class EventRegistry
{
    private readonly Dictionary<string, Type> _schemaToType = new();
    private readonly Dictionary<Type, string> _typeToSchema = new();

    public void Register(Type type)
    {
        var schemaName = type.FullName!;
        Register(type, schemaName);
    }

    public void Register(Type type, string schemaName)
    { 
        ArgumentNullException.ThrowIfNull(schemaName);
        ArgumentNullException.ThrowIfNull(type);

        if (_typeToSchema.ContainsKey(type))
        {
            throw new ArgumentException($"Type {type.FullName} is already registered");
        }
        
        _schemaToType.Add(schemaName, type);
        _typeToSchema.Add(type, schemaName);
    }

    public bool IsRegistered(Type type)
    {
        return _typeToSchema.ContainsKey(type);
    }

    public bool IsRegistered(string schemaName)
    {
        return _schemaToType.ContainsKey(schemaName);
    }

    public string GetSchemaName(Type type)
    {
        if(!_typeToSchema.TryGetValue(type, out var schemaName))
        {
            throw new ArgumentException($"Type {type.FullName} is not registered");
        }

        return schemaName;
    }

    public Type GetType(string schemaName)
    {
        if(!_schemaToType.TryGetValue(schemaName, out var type))
        {
            throw new ArgumentException($"Schema {schemaName} is not registered");
        }

        return type;
    }
}