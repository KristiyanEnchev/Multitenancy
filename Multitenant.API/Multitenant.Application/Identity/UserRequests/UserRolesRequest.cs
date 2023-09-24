namespace Multitenant.Application.Identity.UserRequests
{
    using Multitenant.Models.Identity;

    public class UserRolesRequest
    {
        public List<UserRoleDto> UserRoles { get; set; } = new();
    }
}