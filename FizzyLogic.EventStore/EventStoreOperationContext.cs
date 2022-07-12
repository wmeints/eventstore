using Microsoft.EntityFrameworkCore;

namespace FizzyLogic.EventStore;

public class EventStoreOperationContext<TContext> where TContext : DbContext
{
    public EventStoreOperationContext(EventStoreSchemaRegistry eventStoreSchemaRegistry, TContext dbContext)
    {
        EventStoreSchemaRegistry = eventStoreSchemaRegistry;
        DbContext = dbContext;
    }

    public EventStoreSchemaRegistry EventStoreSchemaRegistry { get; }
    public TContext DbContext { get; }

    public DbSet<EventRecord> Events => DbContext.Set<EventRecord>();

    public DbSet<SnapshotRecord> Snapshots => DbContext.Set<SnapshotRecord>();
}