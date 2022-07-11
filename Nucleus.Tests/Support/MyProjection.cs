namespace Nucleus.Tests.Support;

public class MyProjection: IProjection<MyDbContext>
{
    public int Invocations { get; set; }
    
    public Task Project(IProjectionContext<MyDbContext> context)
    {
        Invocations++;

        return Task.CompletedTask;
    }
}