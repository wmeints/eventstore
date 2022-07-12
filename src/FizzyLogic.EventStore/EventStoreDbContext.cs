using Microsoft.EntityFrameworkCore;

namespace FizzyLogic.EventStore;

public abstract class EventStoreDbContext : DbContext, IEventStoreDbContext
{
    protected EventStoreDbContext()
    {
    }

    protected EventStoreDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<EventRecord> Events => Set<EventRecord>();

    public DbSet<SnapshotRecord> Snapshots => Set<SnapshotRecord>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EventRecordEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new SnapshotRecordEntityTypeConfiguration());
    }
}