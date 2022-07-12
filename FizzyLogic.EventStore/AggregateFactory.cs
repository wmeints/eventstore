using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace FizzyLogic.EventStore;

using StreamConstructorFunc = Func<object, long, IEnumerable<object>, object>;
using SnapshotConstructorFunc = Func<object, long, object, IEnumerable<object>, object>;

public static class AggregateFactory
{
    private static readonly Dictionary<Type, StreamConstructorFunc> StreamingConstructorsCache = new();
    private static readonly Dictionary<Type, SnapshotConstructorFunc> SnapshotConstructorsCache = new();

    public static TAggregate Create<TAggregate, TId>([DisallowNull] TId id, long version, IEnumerable<object> events)
    {
        var constructorFunc = GetOrCreateStreamingConstructor<TAggregate>();
        return (TAggregate) constructorFunc(id, version, events);
    }

    public static TAggregate Create<TAggregate, TId>([DisallowNull] TId id, long version, object snapshot, IEnumerable<object> events)
    {
        var constructorFunc = GetOrCreateSnapshotConstructor<TAggregate>();
        return (TAggregate) constructorFunc(id, version, snapshot, events);
    }

    private static SnapshotConstructorFunc GetOrCreateSnapshotConstructor<TAggregate>()
    {
        if (SnapshotConstructorsCache.TryGetValue(typeof(TAggregate), out var constructorFunc))
        {
            return constructorFunc;
        }

        var aggregateType = typeof(TAggregate);

        var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        var constructor = aggregateType.GetConstructors(bindingFlags)
            .FirstOrDefault(IsSnapshotConstructor);

        if (constructor == null)
        {
            throw new AggregateConstructorException(
                "Can't find a suitable constructor that accepts the aggregate" +
                " Id, version, a snapshot, and a series of events.");
        }

        var parameters = constructor.GetParameters();

        var idParameter = Expression.Parameter(typeof(object));
        var versionParameter = Expression.Parameter(typeof(long));
        var snapshotParameter = Expression.Parameter(typeof(object));
        var eventsParameter = Expression.Parameter(typeof(IEnumerable<object>));

        var callExpression = Expression.New(constructor,
            Expression.Convert(idParameter, parameters[0].ParameterType),
            versionParameter,
            snapshotParameter,
            eventsParameter);

        var constructorLambda = Expression.Lambda<Func<object, long, object, IEnumerable<object>, object>>(
            callExpression, idParameter, versionParameter, snapshotParameter, eventsParameter).Compile();

        SnapshotConstructorsCache.Add(typeof(TAggregate), constructorLambda);

        return constructorLambda;
    }

    private static StreamConstructorFunc GetOrCreateStreamingConstructor<TAggregate>()
    {
        if (StreamingConstructorsCache.TryGetValue(typeof(TAggregate), out var constructorFunc))
        {
            return constructorFunc;
        }

        var aggregateType = typeof(TAggregate);

        var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        var constructor = aggregateType.GetConstructors(bindingFlags)
            .FirstOrDefault(IsEventStreamConstructor);

        if (constructor == null)
        {
            throw new AggregateConstructorException(
                "Can't find a suitable constructor that accepts the aggregate" +
                " Id, version, and a series of events.");
        }

        var parameters = constructor.GetParameters();

        var idParameter = Expression.Parameter(typeof(object));
        var versionParameter = Expression.Parameter(typeof(long));
        var eventsParameter = Expression.Parameter(typeof(IEnumerable<object>));

        var callExpression = Expression.New(constructor,
            Expression.Convert(idParameter, parameters[0].ParameterType),
            versionParameter,
            eventsParameter);

        var constructorLambda = Expression.Lambda<Func<object, long, IEnumerable<object>, object>>(
            callExpression, idParameter, versionParameter, eventsParameter).Compile();

        StreamingConstructorsCache.Add(typeof(TAggregate), constructorLambda);

        return constructorLambda;
    }

    private static bool IsEventStreamConstructor(ConstructorInfo constructorInfo)
    {
        var parameters = constructorInfo.GetParameters();

        if (parameters.Length != 3)
        {
            return false;
        }

        return parameters[1].ParameterType == typeof(long) &&
               parameters[2].ParameterType.IsAssignableTo(typeof(IEnumerable<object>));
    }
    
    private static bool IsSnapshotConstructor(ConstructorInfo constructorInfo)
    {
        var parameters = constructorInfo.GetParameters();

        if (parameters.Length != 4)
        {
            return false;
        }

        return parameters[1].ParameterType == typeof(long) &&
               parameters[2].ParameterType == typeof(object) &&
               parameters[3].ParameterType.IsAssignableTo(typeof(IEnumerable<object>));
    }
}