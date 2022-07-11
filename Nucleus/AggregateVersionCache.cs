namespace Nucleus;

internal static class AggregateVersionCache
{
    private static readonly Dictionary<string, long> CacheData = new();

    public static void Put(string aggregateId, long version)
    {
        CacheData[aggregateId] = version;
    }

    public static long Get(string aggregateId)
    {
        if (!CacheData.TryGetValue(aggregateId, out var version))
        {
            return 0L;
        }

        return version;
    }
}