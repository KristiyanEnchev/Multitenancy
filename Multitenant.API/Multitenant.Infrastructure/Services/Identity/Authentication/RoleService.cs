namespace Multitenant.Infrastructure.Services.Identity.Authentication
{
    using Microsoft.AspNetCore.Identity;
    using Multitenant.Application.Interfaces.Identity;
    using Multitenant.Domain.Entities.Identity;

    public class RoleService : IRoleService
    {
        private readonly RoleManager<UserRole> _roleManager;

        public RoleService(RoleManager<UserRole>? roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<bool> ExistsAsync(string roleName, string? excludeId) =>
            await _roleManager.FindByNameAsync(roleName)
                is UserRole existingRole
                && existingRole.Id != excludeId;
    }
}
