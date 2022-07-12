using Microsoft.EntityFrameworkCore;

namespace FizzyLogic.EventStore;

public interface IEventStoreDbContext
{
    DbSet<EventRecord> Events { get; }
}