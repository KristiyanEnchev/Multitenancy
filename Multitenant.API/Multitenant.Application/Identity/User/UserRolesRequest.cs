namespace Multitenant.Application.Identity.User
{
    using Multitenant.Models.Identity;

    public class UserRolesRequest
    {
        public List<UserRoleDto> UserRoles { get; set; } = new();
    }
}