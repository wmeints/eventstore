using FizzyLogic.EventStore.SampleApp.Domain.Events;

namespace FizzyLogic.EventStore.SampleApp.Domain;

public class Product
{
    private readonly List<object> _pendingEvents = new();
    
    public Guid Id { get; private set; }
    public string Name { get; private set; } = "";
    public string Description { get; private set; } = "";
    public bool IsAvailable { get; private set; }

    public long Version { get; private set; } = 0L;
    
    public IReadOnlyCollection<object> PendingEvents => _pendingEvents;

    public Product(Guid id, string name, string description)
    {
        Id = id;
        Publish(new ProductRegistered(id, name, description));
    }

    protected Product(Guid id, long version, IEnumerable<object> domainEvents)
    {
        Id = id;
        Version = version;
        
        foreach (var evt in domainEvents)
        {
            TryApplyEvent(evt);
        }
    }

    public void UpdateProductInformation(string name, string description)
    {
        Publish(new ProductInformationUpdated(Id, name, description));
    }

    public void Discontinue()
    {
        Publish(new ProductDiscontinued(Id));
    }

    private void Publish(object evt)
    {
        if (TryApplyEvent(evt))
        {
            _pendingEvents.Add(evt);
        }
    }

    private bool TryApplyEvent(object evt)
    {
        switch (evt)
        {
            case ProductRegistered productRegistered:
                Apply(productRegistered);
                break;
            case ProductInformationUpdated productInformationUpdated:
                Apply(productInformationUpdated);
                break;
            case ProductDiscontinued productDiscontinued:
                Apply(productDiscontinued);
                break;
            default:
                return false;
        }

        return true;
    }

    private void Apply(ProductDiscontinued productDiscontinued)
    {
        IsAvailable = false;
    }

    private void Apply(ProductInformationUpdated productInformationUpdated)
    {
        Name = productInformationUpdated.Name;
        Description = productInformationUpdated.Description;
    }

    private void Apply(ProductRegistered productRegistered)
    {
        Name = productRegistered.Name;
        Description = productRegistered.Description;
        IsAvailable = true;
    }
}