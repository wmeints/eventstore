using FizzyLogic.EventStore.SampleApp.Application.Commands;
using FizzyLogic.EventStore.SampleApp.Application.ReadModels;
using FizzyLogic.EventStore.SampleApp.Data;
using FizzyLogic.EventStore.SampleApp.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace FizzyLogic.EventStore.SampleApp.Application.Projections;

public class ProductInfoProjection: IProjection
{
    private readonly ApplicationDbContext _applicationDbContext;

    public ProductInfoProjection(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public async Task Project(object @event)
    {
        switch (@event)
        {
            case ProductRegistered productRegistered:
                await CreateProductInfo(productRegistered);
                break;
            case ProductDiscontinued productDiscontinued:
                await DeleteProductInfo(productDiscontinued);
                break;
            case ProductInformationUpdated productInformationUpdated:
                await UpdateProductInfo(productInformationUpdated);
                break;
        }
    }

    private async Task UpdateProductInfo(ProductInformationUpdated productInformationUpdated)
    {
        var info = await _applicationDbContext.Products.SingleOrDefaultAsync(x => x.Id == productInformationUpdated.Id);

        if (info != null)
        {
            info.Name = productInformationUpdated.Name;
            info.Description = productInformationUpdated.Description;
        }
    }

    private async Task DeleteProductInfo(ProductDiscontinued productDiscontinued)
    {
        var info = await _applicationDbContext.Products.SingleOrDefaultAsync(x => x.Id == productDiscontinued.Id);

        if (info != null)
        {
            _applicationDbContext.Remove(info);
        }
    }

    private async Task CreateProductInfo(ProductRegistered productRegistered)
    {
        await _applicationDbContext.AddAsync(new ProductInfo()
        {
            Id = productRegistered.Id,
            Name = productRegistered.Name,
            Description = productRegistered.Description,
            IsAvailable = true
        });
    }
}