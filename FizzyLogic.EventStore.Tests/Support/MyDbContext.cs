using Microsoft.EntityFrameworkCore;

namespace FizzyLogic.EventStore.Tests.Support;

public class MyDbContext : EventStoreDbContext
{
    public MyDbContext(DbContextOptions options) : base(options)
    {
    }
}