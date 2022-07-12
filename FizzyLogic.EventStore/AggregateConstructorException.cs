namespace FizzyLogic.EventStore;

public class AggregateConstructorException: Exception
{
    public AggregateConstructorException(string message) :base(message)
    {
        
    }
}