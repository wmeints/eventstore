namespace FizzyLogic.EventStore.SampleApp.Domain.Events;

public record ProductRegistered(Guid Id, string Name, string Description);