using Microsoft.EntityFrameworkCore;

namespace Nucleus;

public interface IEventStoreDbContext
{
    DbSet<EventRecord> Events { get; }
}