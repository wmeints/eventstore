namespace FizzyLogic.EventStore;

public interface IProjection
{
    Task Project(object @event);
}
