using FizzyLogic.EventStore.SampleApp.Application.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace FizzyLogic.EventStore.SampleApp.Data;

public class ApplicationDbContext: EventStoreDbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<ProductInfo> Products => Set<ProductInfo>();
}
