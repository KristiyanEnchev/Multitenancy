namespace Multitenant.Domain.Contracts
{
    using Multitenant.Shared.Events;

    public abstract class DomainEvent : IEvent
    {
        public DateTime TriggeredOn { get; protected set; } = DateTime.UtcNow;
    }
}