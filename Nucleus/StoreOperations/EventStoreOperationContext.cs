using Microsoft.EntityFrameworkCore;

namespace Nucleus.StoreOperations;

public class EventStoreOperationContext<TContext> where TContext : DbContext
{
    public EventStoreOperationContext(EventRegistry eventRegistry, TContext dbContext)
    {
        EventRegistry = eventRegistry;
        DbContext = dbContext;
    }

    public EventRegistry EventRegistry { get; }
    public TContext DbContext { get; }

    public DbSet<EventRecord> Events => DbContext.Set<EventRecord>();
}