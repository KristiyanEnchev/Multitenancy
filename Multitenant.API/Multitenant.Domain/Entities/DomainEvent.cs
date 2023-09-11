namespace Multitenant.Domain.Entities
{
    using Multitenant.Shared.Events;

    public abstract class DomainEvent : IEvent
    {
        public DateTime TriggeredOn { get; protected set; } = DateTime.UtcNow;
    }
}