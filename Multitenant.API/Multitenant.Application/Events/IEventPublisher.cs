namespace Multitenant.Application.Events
{
    using Multitenant.Shared.Events;
    using Multitenant.Application.Interfaces.DependencyScope;

    public interface IEventPublisher : ITransientService
    {
        Task PublishAsync(IEvent @event);
    }
}