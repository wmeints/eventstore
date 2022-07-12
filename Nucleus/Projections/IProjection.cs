namespace Nucleus.Projections;

public interface IProjection
{
    Task Project(object @event);
}
