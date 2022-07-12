using Microsoft.EntityFrameworkCore;

namespace FizzyLogic.EventStore;

internal abstract class EventStoreOperation<TContext> where TContext : DbContext
{
    public abstract Task ExecuteAsync(EventStoreOperationContext<TContext> context);
    
    public abstract IEnumerable<object> Events { get; }
}