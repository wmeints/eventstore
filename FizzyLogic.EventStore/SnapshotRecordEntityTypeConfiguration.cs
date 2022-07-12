using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FizzyLogic.EventStore;

public class SnapshotRecordEntityTypeConfiguration: IEntityTypeConfiguration<SnapshotRecord>
{
    public void Configure(EntityTypeBuilder<SnapshotRecord> builder)
    {
        builder.HasIndex(x => new {x.AggregateId, x.Sequence}).IsUnique();
        builder.Property(x => x.AggregateId).HasMaxLength(255).IsRequired();
        builder.Property(x => x.SnapshotType).HasMaxLength(255).IsRequired();
        builder.Property(x => x.SnapshotData).IsRequired();
    }
}