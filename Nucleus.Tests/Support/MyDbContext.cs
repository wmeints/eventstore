using Microsoft.EntityFrameworkCore;

namespace Nucleus.Tests.Support;

public class MyDbContext : EventStoreDbContext
{
    public MyDbContext(DbContextOptions options) : base(options)
    {
    }
}