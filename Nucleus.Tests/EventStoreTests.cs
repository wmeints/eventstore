using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Nucleus.Tests;

public class EventStoreTests
{
    private readonly IServiceProvider _serviceProvider;

    public EventStoreTests()
    {
        if (!EventRegistry.IsRegistered(typeof(MyOtherEvent)))
        {
            EventRegistry.Register(typeof(MyOtherEvent));
        }
        
        var services = new ServiceCollection();

        services.AddDbContext<MyDbContext>(options =>
        {
            options.UseInMemoryDatabase($"Test-{Guid.NewGuid()}");
        });
        
        services.AddEventStore<MyDbContext>();
        
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task CanAppendEventsToNonExistingStream()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        
        var eventStore = scope.ServiceProvider.GetRequiredService<IEventStore>();

        await eventStore.AppendAsync(Guid.NewGuid(), 0L, new[] { new MyOtherEvent() });
        await dbContext.SaveChangesAsync();
        
        var events = await dbContext.Events.ToListAsync();
        
        Assert.Single(events);
    }

    [Fact]
    public async Task CanAppendEventsToExistingStream()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        
        var eventStore = scope.ServiceProvider.GetRequiredService<IEventStore>();

        var aggregateId = Guid.NewGuid();
        await eventStore.CreateStreamAsync(aggregateId, new[] { new MyOtherEvent() });
        await dbContext.SaveChangesAsync();
        
        await eventStore.AppendAsync(aggregateId, 1L, new[] { new MyOtherEvent() });
        await dbContext.SaveChangesAsync();
        
        var events = await dbContext.Events.ToListAsync();
        
        Assert.Equal(2, events.Count);
    }

    [Fact]
    public async Task HandlesConcurrencyProblems()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        
        var eventStore = scope.ServiceProvider.GetRequiredService<IEventStore>();
        
        var aggregateId = Guid.NewGuid();
        await eventStore.CreateStreamAsync(aggregateId, new[] { new MyOtherEvent(), new MyOtherEvent() });
        await dbContext.SaveChangesAsync();

        await Assert.ThrowsAsync<DBConcurrencyException>(async () =>
        {
            await eventStore.AppendAsync(aggregateId, 1L, new[] { new MyOtherEvent() });
        });
    }
    
    [Fact]
    public async Task CanCreateStream()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        
        var eventStore = scope.ServiceProvider.GetRequiredService<IEventStore>();
        
        await eventStore.CreateStreamAsync(Guid.NewGuid(), new[] { new MyOtherEvent() });
        await dbContext.SaveChangesAsync();
        
        var events = await dbContext.Events.ToListAsync();

        Assert.Single(events);
    }
    
    [Fact]
    public async Task CantCreateSameStreamTwice()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        
        var eventStore = scope.ServiceProvider.GetRequiredService<IEventStore>();

        var aggregateId = Guid.NewGuid();
        
        await eventStore.CreateStreamAsync(aggregateId, new[] { new MyOtherEvent() });
        await dbContext.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await eventStore.CreateStreamAsync(aggregateId, new[] { new MyOtherEvent() });
            await dbContext.SaveChangesAsync();
        });
        
        var events = await dbContext.Events.ToListAsync();

        Assert.Single(events);
    }
}