using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FizzyLogic.EventStore;

public static class NucleusExtensions
{
    public static void AddEventStore<TContext>(this IServiceCollection services, Action<EventStoreSetup<TContext>>? configure = null) where TContext : DbContext
    {
        var setup = new EventStoreSetup<TContext>(services);
        
        configure?.Invoke(setup);

        var eventStoreServiceType = typeof(EventStore<>).MakeGenericType(typeof(TContext));
        var projectionEngineServiceType = typeof(ProjectionEngine<>).MakeGenericType(typeof(TContext));
        
        services.AddSingleton(setup.EventStoreSchemaRegistry);
        
        services.AddScoped(typeof(IEventStore), eventStoreServiceType);
        services.AddScoped(projectionEngineServiceType);
    }
}