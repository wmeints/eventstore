namespace Nucleus;

public interface IProjection
{
    Task Project(object @event);
}
