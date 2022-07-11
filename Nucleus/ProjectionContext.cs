using Microsoft.EntityFrameworkCore;

namespace Nucleus;

public class ProjectionContext<TContext>: IProjectionContext<TContext> where TContext : DbContext
{
    public ProjectionContext(TContext dbContext)
    {
        DbContext = dbContext;
    }

    public TContext DbContext { get; }
}