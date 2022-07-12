using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nucleus.Tests.Support;

namespace Nucleus.Tests;

public class EventStoreTests
{
    private readonly IServiceProvider _serviceProvider;

    public EventStoreTests()
    {
        var services = new ServiceCollection();

        services.AddDbContext<MyDbContext>(options => { options.UseInMemoryDatabase($"Test-{Guid.NewGuid()}"); });

        services.AddEventStore<MyDbContext>(options =>
        {
            options.RegisterEvent<MyOtherEvent>();
            options.RegisterSnapshot<MySnapshot>();
        });

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task CanAppendEventsToNonExistingStream()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();

        var eventStore = scope.ServiceProvider.GetRequiredService<IEventStore>();

        eventStore.AppendToStream(Guid.NewGuid(), 0L, new[] {new MyOtherEvent()});
        await eventStore.SaveChangesAsync();

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
        eventStore.CreateStream(aggregateId, new[] {new MyOtherEvent()});
        await eventStore.SaveChangesAsync();

        eventStore.AppendToStream(aggregateId, 1L, new[] {new MyOtherEvent()});
        await eventStore.SaveChangesAsync();

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

        eventStore.CreateStream(aggregateId, new[] {new MyOtherEvent(), new MyOtherEvent()});
        await dbContext.SaveChangesAsync();

        await Assert.ThrowsAsync<ConcurrencyException>(async () =>
        {
            eventStore.AppendToStream(aggregateId, 1L, new[] {new MyOtherEvent()});
            await eventStore.SaveChangesAsync();
        });
    }

    [Fact]
    public async Task CanCreateStream()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();

        var eventStore = scope.ServiceProvider.GetRequiredService<IEventStore>();

        eventStore.CreateStream(Guid.NewGuid(), new[] {new MyOtherEvent()});
        await eventStore.SaveChangesAsync();

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

        eventStore.CreateStream(aggregateId, new[] {new MyOtherEvent()});
        await eventStore.SaveChangesAsync();

        await Assert.ThrowsAsync<ConcurrencyException>(async () =>
        {
            eventStore.CreateStream(aggregateId, new[] {new MyOtherEvent()});
            await eventStore.SaveChangesAsync();
        });

        var events = await dbContext.Events.ToListAsync();

        Assert.Single(events);
    }

    [Fact]
    public async Task CanSaveSnapshots()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();

        var eventStore = scope.ServiceProvider.GetRequiredService<IEventStore>();
        var aggregateId = Guid.NewGuid();
        
        eventStore.CreateStream(aggregateId, new[] { new MyOtherEvent() });
        eventStore.SaveSnapshot(aggregateId, 1, new MySnapshot() {Message = "Hello world"});

        await eventStore.SaveChangesAsync();

        var snapshots = await dbContext.Snapshots.ToListAsync();

        Assert.Single(snapshots);
    }

    [Fact]
    public async Task CanRestoreStateFromEventStream()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();

        var eventStore = scope.ServiceProvider.GetRequiredService<IEventStore>();
        var aggregateId = Guid.NewGuid();

        eventStore.CreateStream(aggregateId, new[] { new MyOtherEvent() });
        await eventStore.SaveChangesAsync();

        var result = await eventStore.GetAsync<MyAggregate, Guid>(aggregateId);
        
        Assert.NotNull(result);
    }

    [Fact]
    public async Task CanRestoreFromSnapshot()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();

        var eventStore = scope.ServiceProvider.GetRequiredService<IEventStore>();
        var aggregateId = Guid.NewGuid();

        eventStore.CreateStream(aggregateId, new[] { new MyOtherEvent() });
        eventStore.SaveSnapshot(aggregateId, 1L, new MySnapshot() { Message = "Test"});
        eventStore.AppendToStream(aggregateId, 1L, new[] { new MyOtherEvent() });
        
        await eventStore.SaveChangesAsync();

        var result = await eventStore.GetAsync<MyAggregate, Guid>(aggregateId);
        
        Assert.NotNull(result);
        Assert.Equal("Test", result.Message);
        Assert.Single(result.Events);
    }
}