using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FizzyLogic.EventStore;

public class EventStoreSetup<TContext> where TContext : DbContext
{
    private readonly EventStoreSchemaRegistry _eventStoreSchemaRegistry = new();
    private readonly IServiceCollection _services;

    public EventStoreSetup(IServiceCollection services)
    {
        _services = services;
    }

    public void RegisterEvent<T>(string? schemaName = null)
    {
        if (string.IsNullOrEmpty(schemaName))
        {
            _eventStoreSchemaRegistry.Register(typeof(T));    
        }
        else
        {
            _eventStoreSchemaRegistry.Register(typeof(T), schemaName);
        }
    }

    public void RegisterSnapshot<T>(string? schemaName = null)
    {
        if (string.IsNullOrEmpty(schemaName))
        {
            _eventStoreSchemaRegistry.Register(typeof(T));    
        }
        else
        {
            _eventStoreSchemaRegistry.Register(typeof(T), schemaName);
        }
    }

    public void RegisterProjection<T>() where T : class, IProjection
    {
        _services.AddScoped<IProjection, T>();
    }

    public EventStoreSchemaRegistry EventStoreSchemaRegistry => _eventStoreSchemaRegistry;
}