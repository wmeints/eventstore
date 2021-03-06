using FizzyLogic.EventStore.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FizzyLogic.EventStore.Tests;

public class ProjectionTests
{
    private readonly IServiceProvider _serviceProvider;

    public ProjectionTests()
    {
        var services = new ServiceCollection();

        services.AddDbContext<MyDbContext>(options => { options.UseInMemoryDatabase($"Test-{Guid.NewGuid()}"); });
        services.AddEventStore<MyDbContext>(config =>
        {
            config.RegisterEvent<MyOtherEvent>();
            config.RegisterProjection<MyProjection>();
        });

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task CanPerformInlineProjections()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();

        var projection = (MyProjection)scope.ServiceProvider.GetRequiredService<IProjection>();
        var eventStore = scope.ServiceProvider.GetRequiredService<IEventStore>();

        eventStore.CreateStream(Guid.NewGuid(), new[] { new MyOtherEvent() });
        await eventStore.SaveChangesAsync();
        
        Assert.Equal(1, projection.Invocations);
    }
}