using Microsoft.EntityFrameworkCore;

namespace Nucleus.Tests;

public class MyDbContext : EventStoreDbContext
{
    public MyDbContext(DbContextOptions options) : base(options)
    {
    }
}