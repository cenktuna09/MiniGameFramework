namespace Core.Architecture
{
    /// <summary>
    /// Marker interface for all events in the system.
    /// Used to identify classes that represent events in the event-driven architecture.
    /// </summary>
    public interface IEvent
    {
        // Marker interface - no members required
        // This interface is used to identify classes that represent events
        // and can be published through the IEventBus system.
    }
} 