using FizzyLogic.EventStore.SampleApp.Application.QueryHandlers;
using FizzyLogic.EventStore.SampleApp.Application.ReadModels;
using FizzyLogic.EventStore.SampleApp.Data;
using Microsoft.EntityFrameworkCore;

namespace FizzyLogic.EventStore.SampleApp.Application.Queries;

public class FindProductByIdQueryHandler
{
    private readonly ApplicationDbContext _applicationDbContext;

    public FindProductByIdQueryHandler(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public async Task<ProductInfo?> ExecuteAsync(FindProductById query)
    {
        return await _applicationDbContext.Products.SingleOrDefaultAsync(x => x.Id == query.Id);
    }
}