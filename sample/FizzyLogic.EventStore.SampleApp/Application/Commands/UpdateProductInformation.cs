namespace FizzyLogic.EventStore.SampleApp.Application.Commands;

public record UpdateProductInformation(Guid Id, string Name, string Description);