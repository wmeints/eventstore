using FizzyLogic.EventStore.SampleApp.Application.Commands;
using FizzyLogic.EventStore.SampleApp.Domain;

namespace FizzyLogic.EventStore.SampleApp.Application.CommandHandlers;

public class RegisterProductCommandHandler
{
    private readonly IEventStore _eventStore;

    public RegisterProductCommandHandler(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task ExecuteAsync(RegisterProduct cmd)
    {
        var product = new Product(cmd.Id, cmd.Name, cmd.Description);

        _eventStore.CreateStream(product.Id, product.PendingEvents);
        await _eventStore.SaveChangesAsync();
    }
}