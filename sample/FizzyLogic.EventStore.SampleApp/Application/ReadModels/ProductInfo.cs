namespace FizzyLogic.EventStore.SampleApp.Application.ReadModels;

public class ProductInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get;set; }
    public bool IsAvailable { get; set; }
}