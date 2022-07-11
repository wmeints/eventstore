namespace Nucleus.Tests;

public class MyProjection: IProjection<MyDbContext>
{
    public int Invocations { get; set; }
    
    public Task Project(IProjectionContext<MyDbContext> context)
    {
        Invocations++;

        return Task.CompletedTask;
    }
}