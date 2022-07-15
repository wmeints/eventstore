using FizzyLogic.EventStore.SampleApp.Application.Commands;
using FizzyLogic.EventStore.SampleApp.Data;
using FizzyLogic.EventStore.SampleApp.Domain;

namespace FizzyLogic.EventStore.SampleApp.Application.CommandHandlers;

public class DiscontinueProductCommandHandler
{
    private readonly IEventStore _eventStore;

    public DiscontinueProductCommandHandler(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task ExecuteAsync(DiscontinueProduct cmd)
    {
        var product = await _eventStore.GetAsync<Product, Guid>(cmd.Id);

        if (product == null)
        {
            throw new Exception($"Can't find product with ID {cmd.Id}");
        }

        product.Discontinue();

        _eventStore.AppendToStream(product.Id, product.Version, product.PendingEvents);
        await _eventStore.SaveChangesAsync();
    }
}