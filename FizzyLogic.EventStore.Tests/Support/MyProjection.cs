namespace FizzyLogic.EventStore.Tests.Support;

public class MyProjection: IProjection
{
    public int Invocations { get; set; }
    
    public Task Project(object @event)
    {
        Invocations++;

        return Task.CompletedTask;
    }
}