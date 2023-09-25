namespace Multitenant.Application.Identity.UserIdentity
{
    using Multitenant.Models.Identity;

    public class UserRolesRequest
    {
        public List<UserRoleDto> UserRoles { get; set; } = new();
    }
}