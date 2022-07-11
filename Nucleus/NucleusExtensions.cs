using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Nucleus;

public static class NucleusExtensions
{
    public static void AddEventStore<TContext>(this IServiceCollection services, Action<EventStoreSetup>? configure = null) where TContext : DbContext
    {
        configure?.Invoke(EventStoreSetup.Instance);

        var serviceType = typeof(EventStore<>).MakeGenericType(typeof(TContext));
        services.AddScoped(typeof(IEventStore), serviceType);
    }
}