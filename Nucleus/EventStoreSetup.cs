using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Nucleus;

public class EventStoreSetup<TContext> where TContext : DbContext
{
    private readonly IServiceCollection _services;

    public EventStoreSetup(IServiceCollection services)
    {
        _services = services;
    }

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

    public void RegisterProjection<T>() where T : class, IProjection<TContext>
    {
        _services.AddScoped<IProjection<TContext>, T>();
    }

}