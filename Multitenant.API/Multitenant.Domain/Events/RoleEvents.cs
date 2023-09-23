namespace Multitenant.Domain.Events
{
    using Multitenant.Domain.Entities;

    public abstract class RoleEvent : DomainEvent
    {
        public string RoleId { get; set; } = default!;
        public string RoleName { get; set; } = default!;
        protected RoleEvent(string roleId, string roleName) =>
            (RoleId, RoleName) = (roleId, roleName);
    }

    public class ApplicationRoleCreatedEvent : RoleEvent
    {
        public ApplicationRoleCreatedEvent(string roleId, string roleName)
            : base(roleId, roleName)
        {
        }
    }

    public class ApplicationRoleUpdatedEvent : RoleEvent
    {
        public bool PermissionsUpdated { get; set; }

        public ApplicationRoleUpdatedEvent(string roleId, string roleName, bool permissionsUpdated = false)
            : base(roleId, roleName) =>
            PermissionsUpdated = permissionsUpdated;
    }

    public class ApplicationRoleDeletedEvent : RoleEvent
    {
        public bool PermissionsUpdated { get; set; }

        public ApplicationRoleDeletedEvent(string roleId, string roleName)
            : base(roleId, roleName)
        {
        }
    }
}