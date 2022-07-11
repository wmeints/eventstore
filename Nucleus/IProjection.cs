namespace Nucleus;

public interface IProjection<TContext>
{
    Task Project(IProjectionContext<TContext> context);
}

public interface IProjectionContext<TContext>
{
    
}