using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Nucleus;

public class ProjectionEngine<TContext> where TContext : DbContext
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TContext _dbContext;

    public ProjectionEngine(IServiceProvider serviceProvider, TContext dbContext)
    {
        _serviceProvider = serviceProvider;
        _dbContext = dbContext;
    }

    public async Task RunAsync(IEnumerable<object> events)
    {
        var eventsList = events.ToList();
        var projectors = _serviceProvider.GetServices<IProjection<TContext>>().ToList();
        var projectionContext = new ProjectionContext<TContext>(_dbContext);

        // Enumerating events and then for each event the projectors ensures that all projectors
        // see the same timeline of events.
        foreach (var @event in eventsList)
        {
            foreach (var projector in projectors)
            {
                await projector.Project(projectionContext);
            }
        }
    }
}