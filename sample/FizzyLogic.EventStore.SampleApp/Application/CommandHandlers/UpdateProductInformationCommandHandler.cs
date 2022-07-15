using FizzyLogic.EventStore.SampleApp.Application.Commands;
using FizzyLogic.EventStore.SampleApp.Domain;

namespace FizzyLogic.EventStore.SampleApp.Application.CommandHandlers;

public class UpdateProductInformationCommandHandler
{
    private readonly IEventStore _eventStore;

    public UpdateProductInformationCommandHandler(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task ExecuteAsync(UpdateProductInformation cmd)
    {
        var product = await _eventStore.GetAsync<Product, Guid>(cmd.Id);

        if (product == null)
        {
            throw new Exception($"Can't find product with ID {cmd.Id}");
        }

        product.UpdateProductInformation(cmd.Name,cmd.Description);

        _eventStore.AppendToStream(product.Id, product.Version, product.PendingEvents);
        await _eventStore.SaveChangesAsync();
    }
}