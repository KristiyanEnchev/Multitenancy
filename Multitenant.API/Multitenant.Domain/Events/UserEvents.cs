namespace Multitenant.Domain.Events
{
    using Multitenant.Domain.Entities;

    public abstract class UserEvent : DomainEvent
    {
        public string UserId { get; set; } = default!;

        protected UserEvent(string userId) => UserId = userId;
    }

    public class ApplicationUserCreatedEvent : UserEvent
    {
        public ApplicationUserCreatedEvent(string userId)
            : base(userId)
        {
        }
    }

    public class ApplicationUserUpdatedEvent : UserEvent
    {
        public bool RolesUpdated { get; set; }

        public ApplicationUserUpdatedEvent(string userId, bool rolesUpdated = false)
            : base(userId) =>
            RolesUpdated = rolesUpdated;
    }
}