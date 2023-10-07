namespace Multitenant.Infrastructure.Services.Identity.Authentication.RoleService
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    using Mapster;

    using Finbuckle.MultiTenant;

    using Multitenant.Application.Interfaces.Identity;
    using Multitenant.Application.Events;
    using Multitenant.Application.Exceptions;
    using Multitenant.Application.Identity.Role;
    using Multitenant.Application.Interfaces.Utility.User;
    using Multitenant.Domain.Events;
    using Multitenant.Infrastructure.Services.Tenant.Context;
    using Multitenant.Models.Identity;
    using Multitenant.Domain.Entities.Identity;
    using Multitenant.Shared.ClaimsPrincipal;

    internal class RoleService : IRoleService
    {
        private readonly RoleManager<UserRole> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly IEventPublisher _events;

        public RoleService(
            RoleManager<UserRole> roleManager,
            UserManager<User> userManager,
            ApplicationDbContext db,
            IEventPublisher events)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _db = db;
            _events = events;
        }

        public async Task<List<RoleDto>> GetListAsync(CancellationToken cancellationToken) =>
            (await _roleManager.Roles.ToListAsync(cancellationToken))
                .Adapt<List<RoleDto>>();

        public async Task<int> GetCountAsync(CancellationToken cancellationToken) =>
            await _roleManager.Roles.CountAsync(cancellationToken);

        public async Task<bool> ExistsAsync(string roleName, string? excludeId) =>
            await _roleManager.FindByNameAsync(roleName)
                is UserRole existingRole
                && existingRole.Id != excludeId;

        public async Task<RoleDto> GetByIdAsync(string id) =>
            await _db.Roles.SingleOrDefaultAsync(x => x.Id == id) is { } role
                ? role.Adapt<RoleDto>()
                : throw new NotFoundException("Role Not Found");

        public async Task<RoleDto> GetByNameAsync(string name) =>
            await _roleManager.FindByNameAsync(name) is { } role
                ? role.Adapt<RoleDto>()
                : throw new NotFoundException("Role Not Found");

        public async Task<RoleDto> GetByNameWithPermissionsAsync(string roleName, CancellationToken cancellationToken)
        {
            var role = await GetByNameAsync(roleName);

            role.Permissions = await _db.RoleClaims
                .Where(c => c.RoleId == role.Id && c.ClaimType == LocalAppClaims.Permission)
                .Select(c => c.ClaimValue!)
                .ToListAsync(cancellationToken);

            return role;
        }

        public async Task<RoleDto> GetByIdWithPermissionsAsync(string roleId, CancellationToken cancellationToken)
        {
            var role = await GetByIdAsync(roleId);

            role.Permissions = await _db.RoleClaims
                .Where(c => c.RoleId == roleId && c.ClaimType == LocalAppClaims.Permission)
                .Select(c => c.ClaimValue!)
                .ToListAsync(cancellationToken);

            return role;
        }

        public async Task<string> CreateOrUpdateAsync(CreateOrUpdateRoleRequest request)
        {
            if (string.IsNullOrEmpty(request.Id))
            {
                // Create a new role.
                var role = new UserRole(request.Name, request.Description);
                var result = await _roleManager.CreateAsync(role);

                if (!result.Succeeded)
                {
                    throw new InternalServerException("Register role failed", GetErrors(result));
                }

                await _events.PublishAsync(new ApplicationRoleCreatedEvent(role.Id, role.Name!));

                return string.Format("Role {0} Created.", request.Name);
            }
            else
            {
                // Update an existing role.
                var role = await _roleManager.FindByIdAsync(request.Id);

                _ = role ?? throw new NotFoundException("Role Not Found");

                if (Roles.IsDefault(role.Name!))
                {
                    throw new ConflictException(string.Format("Not allowed to modify {0} Role.", role.Name));
                }

                role.Name = request.Name;
                role.NormalizedName = request.Name.ToUpperInvariant();
                role.Description = request.Description;
                var result = await _roleManager.UpdateAsync(role);

                if (!result.Succeeded)
                {
                    throw new InternalServerException("Update role failed", GetErrors(result));
                }

                await _events.PublishAsync(new ApplicationRoleUpdatedEvent(role.Id, role.Name));

                return string.Format("Role {0} Updated.", role.Name);
            }
        }

        public async Task<string> DeleteAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            _ = role ?? throw new NotFoundException("Role Not Found");

            if (Roles.IsDefault(role.Name!))
            {
                throw new ConflictException(string.Format("Not allowed to delete {0} Role.", role.Name));
            }

            if ((await _userManager.GetUsersInRoleAsync(role.Name!)).Count > 0)
            {
                throw new ConflictException(string.Format("Not allowed to delete {0} Role as it is being used.", role.Name));
            }

            await _roleManager.DeleteAsync(role);

            await _events.PublishAsync(new ApplicationRoleDeletedEvent(role.Id, role.Name!));

            return string.Format("Role {0} Deleted.", role.Name);
        }

        public static List<string> GetErrors(IdentityResult result) =>
            result.Errors.Select(e => e.Description.ToString()).ToList();
    }
}