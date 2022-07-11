using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nucleus;

public interface IEventStoreDbContext
{
    DbSet<EventRecord> Events { get; }
}

public class EventRecordEntityTypeConfiguration : IEntityTypeConfiguration<EventRecord>
{
    public void Configure(EntityTypeBuilder<EventRecord> builder)
    {
        builder.HasIndex(x => new { x.AggregateId, x.Sequence }).IsUnique();
        builder.Property(x => x.AggregateId).HasMaxLength(255).IsRequired();
        builder.Property(x => x.EventType).HasMaxLength(255).IsRequired();
        builder.Property(x => x.EventData).IsRequired();
    }
}