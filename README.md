# FizzyLogic.EventStore

FizzyLogic.EventStore is a .NET library for building event-sourced applications
with as little ceremony as possible. You can use Nucleus with an existing
relational database. You can even combine event-sourcing with regular SQL
tables if you want to.

## Getting started

To get started, you'll need to add the `FizzyLogic.EventStore` package to your
project. You can do this through the terminal using the following command:

```shell
dotnet add package FizzyLogic.EventStore
```

This library allows you to use event sourcing with a relational database.
The package uses an existing Entity Framework Core data context class with two
new entities `EventRecord` and `SnapshotRecord`. 

You have two options to extend your database for event sourcing. First, you can
modify an existing `DbContext` class:

```csharp
public class MyDbContext : DbContext, IEventStoreDbContext
{
    // ... Your existing code.
  
    public DbSet<EventRecord> Events => Set<EventRecord>();
    public DbSet<SnapshotRecord> Snapshots => Set<SnapshotRecord>();
  
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EventRecordEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new SnapshotRecordEntityTypeConfiguration());
    }
}
```

Another option is to derive your DbContext class from the `EventStoreDbContext`
class:

```csharp
public class MyDbContext: EventStoreDbContext 
{
    public MyDbContext(DbContextOptions options) : base(options)
    {
    }
}
```

You can use any relational database you like as long as there's a provider for
it for Entity Framework Core.

Once you have a DbContext available, you can configure the event store by adding
the following logic to your `Program.cs` file:

```csharp
builder.Services.AddDbContext<MyDbContext>(options => 
{ 
    options.UseInMemoryDatabase($"Test-{Guid.NewGuid()}"); 
});

builder.Services.AddEventStore<MyDbContext>(options =>
{
    options.RegisterEvent<MyOtherEvent>();
    options.RegisterSnapshot<MySnapshot>();
});
```

The first line in the snippet, where we call `AddDbContext` configures your
DbContext class as normal. The third line in the snippet makes the `IEventStore`
interface available in your application. 

You can provide additional configuration as part of the `AddEventStore` method
call. Two samples of that are shown in the snippet:

* Calling `RegisterEvent` will map a .NET type as an event that can be used in
  the event store. You can specify a custom name if you want. You'll get an
  error message when you try to save events that haven't been mapped.
* Calling `RegisterSnapshot` will map a .NET type as a snapshot type. As with
  events, you'll get an error when you're using an unmapped snapshot type to
  create a snapshot in the event store.
  
Once you have the event store configured, you can request in a command handler
or event handler to use event-sourcing:

```csharp
public class UpdateProductInformationCommandHandler
{
    private readonly IEventStore<MyDbContext> _eventStore;

    public MyCommandHandler(IEventStore<MyDbContext> eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task ExecuteAsync(UpdateProductInformation cmd) 
    {
        var aggregate = await _eventStore.GetAsync<Product, Guid>(cmd.Id);
        aggregate.UpdateProductInformation(cmd.Name, cmd.Description);

        _eventStore.AppendToStream(aggregate.Id, 
            aggregate.Version, aggregate.PendingEvents);

        await _eventStore.SaveChangesAsync();
    }
}
```

The code fragment above shows several operations that you can perform with
the event store:

* You can use the `GetAsync` method to load an event stream and reconstruct
  an aggregate. The only requirement is that the aggregate has a constructor
  that accepts an ID, a parameter of type `Int64` for the version, and a
  parameter of type `IEnumerable<object>` for the events. The aggregate is
  responsible for replaying the events to restore its state.
* Next, you can use the `AppendToStream` operation to append events to an
  existing event stream. You need to provide the ID of the aggregate, the
  current version for optimistic concurrency control, and a list of events to
  persist.
* Finally, you can use the `SaveChangesAsync` operation to save the changes to
  the database. 

Please check the [sample](./sample) folder to see how you can use the event
store in a typical ASP.NET Core application. 

## Documentation

You can find the documentation in the [docs](./docs) folder of the repository.

## Contributing

Feel free to open an issue if you find a bug or like to propose an improvement.