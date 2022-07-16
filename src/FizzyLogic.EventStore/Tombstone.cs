namespace FizzyLogic.EventStore;

public class Tombstone
{
    public static Tombstone Instance { get; } = new();
}