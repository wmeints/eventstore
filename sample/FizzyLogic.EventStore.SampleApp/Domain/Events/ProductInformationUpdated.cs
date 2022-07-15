namespace FizzyLogic.EventStore.SampleApp.Domain.Events;

public record ProductInformationUpdated(Guid Id, string Name, string Description);