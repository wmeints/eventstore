namespace FizzyLogic.EventStore.SampleApp.Application.Commands;

public record RegisterProduct(Guid Id, string Name, string Description);