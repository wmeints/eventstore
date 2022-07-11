using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Nucleus;

public static class NucleusExtensions
{
    public static void AddEventStore<TContext>(this IServiceCollection services, Action<SetupEventStore>? configure = null) where TContext : DbContext
    {
        configure?.Invoke(SetupEventStore.Instance);

        var serviceType = typeof(EventStore<>).MakeGenericType(typeof(TContext));
        services.AddScoped(typeof(IEventStore), serviceType);
    }
}