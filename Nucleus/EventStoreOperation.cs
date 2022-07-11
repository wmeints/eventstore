using Microsoft.EntityFrameworkCore;

namespace Nucleus;

internal abstract class EventStoreOperation<TContext> where TContext : DbContext
{
    public abstract Task ExecuteAsync(TContext context);
}