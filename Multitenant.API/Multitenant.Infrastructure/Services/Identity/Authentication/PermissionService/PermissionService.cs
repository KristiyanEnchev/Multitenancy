namespace Multitenant.Infrastructure.Services.Identity.Authentication.PermissionService
{
    using System.Security.Claims;

    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    using Finbuckle.MultiTenant;

    using Mapster;

    using Multitenant.Application.Events;
    using Multitenant.Domain.Entities.Identity;
    using Multitenant.Application.Interfaces.Utility.User;
    using Multitenant.Infrastructure.Services.Tenant.Context;
    using Multitenant.Application.Exceptions;
    using Multitenant.Shared.ClaimsPrincipal;
    using Multitenant.Models.Permission;
    using Multitenant.Domain.Events;
    using Multitenant.Shared.Constants.Multitenancy;
    using Multitenant.Application.Interfaces.Identity;
    using Multitenant.Application.Identity.Permission;

    public class PermissionService : IPermissionService
    {
        private readonly RoleManager<UserRole> _roleManager;
        private readonly ApplicationDbContext _db;
        private readonly ICurrentUser _currentUser;
        private readonly ITenantInfo _currentTenant;
        private readonly IEventPublisher _events;

        public PermissionService(
            RoleManager<UserRole> roleManager,
            ApplicationDbContext db,
            ICurrentUser currentUser,
            ITenantInfo currentTenant,
            IEventPublisher events)
        {
            _roleManager = roleManager;
            _db = db;
            _currentUser = currentUser;
            _currentTenant = currentTenant;
            _events = events;
        }

        public async Task<List<PermissionDto>> GetListAsync(CancellationToken cancellationToken) =>
           (await _db.RoleClaims.ToListAsync(cancellationToken))
               .Adapt<List<PermissionDto>>();

        public async Task<int> GetCountAsync(CancellationToken cancellationToken) =>
            await _db.RoleClaims.CountAsync(cancellationToken);

        public async Task<bool> ExistsAsync(string permissionName, int? excludeId) =>
            await _db.RoleClaims.SingleOrDefaultAsync(x => x.ClaimValue == permissionName)
                is RoleClaim existingPermission
                && existingPermission.Id != excludeId;

        public async Task<bool> ExistsByIdAsync(int permissionId, int? excludeId) =>
            await _db.RoleClaims.SingleOrDefaultAsync(x => x.Id == permissionId)
                is RoleClaim existingPermission
                && existingPermission.Id != excludeId;

        public async Task<bool> RoleHasPermission(string roleId, int permissionId)
        {
            var permission = await GetByIdAsync(permissionId) ?? throw new NotFoundException("Permission Not Found");
            return await _db.RoleClaims.AnyAsync(x => x.ClaimValue == permission.ClaimValue && x.RoleId == roleId);
        }

        public async Task<PermissionDto> GetByIdAsync(int id) =>
            await _db.RoleClaims.SingleOrDefaultAsync(x => x.Id == id) is { } permission
                ? permission.Adapt<PermissionDto>()
                : throw new NotFoundException("Permission Not Found");

        public async Task<PermissionDto> GetByNameAsync(string name) =>
            await _db.RoleClaims.SingleOrDefaultAsync(x => x.ClaimValue == name) is { } permission
                ? permission.Adapt<PermissionDto>()
                : throw new NotFoundException("Permission Not Found");

        public async Task<string> UpdatePermissionsAsync(UpdateRolePermissionsRequest request, CancellationToken cancellationToken)
        {
            var role = await _roleManager.FindByIdAsync(request.RoleId);
            _ = role ?? throw new NotFoundException("Role Not Found");
            if (role.Name == Roles.Admin)
            {
                throw new ConflictException("Not allowed to modify Permissions for this Role.");
            }

            if (_currentTenant.Id != MultitenancyConstants.Root.Id)
            {
                // Remove Root Permissions if the Role is not created for Root Tenant.
                request.Permissions.RemoveAll(u => u.StartsWith("Permissions.Root."));
            }

            var currentClaims = await _roleManager.GetClaimsAsync(role);

            // Remove permissions that were previously selected
            foreach (var claim in currentClaims.Where(c => !request.Permissions.Any(p => p == c.Value)))
            {
                var removeResult = await _roleManager.RemoveClaimAsync(role, claim);
                if (!removeResult.Succeeded)
                {
                    throw new InternalServerException("Update permissions failed.", GetErrors(removeResult));
                }
            }

            // Add all permissions that were not previously selected
            foreach (string permission in request.Permissions.Where(c => !currentClaims.Any(p => p.Value == c)))
            {
                if (!string.IsNullOrEmpty(permission))
                {
                    _db.RoleClaims.Add(new RoleClaim
                    {
                        RoleId = role.Id,
                        ClaimType = LocalAppClaims.Permission,
                        ClaimValue = permission,
                        CreatedBy = _currentUser.GetUserId().ToString()
                    });
                    await _db.SaveChangesAsync(cancellationToken);
                }
            }

            await _events.PublishAsync(new ApplicationRoleUpdatedEvent(role.Id, role.Name!, true));

            return "Permissions Updated.";
        }

        public async Task<string> AddPermissionToRoleAsync(string roleId, int permissionToAdd)
        {
            // Retrieve the role
            var role = await _roleManager.FindByIdAsync(roleId) ?? throw new NotFoundException("Role Not Found");

            var permission = await GetByIdAsync(permissionToAdd) ?? throw new NotFoundException("Permission Not Found");

            if (permission.ClaimValue != null)
            {
                var claim = new Claim(LocalAppClaims.Permission, permission.ClaimValue);
                var addResult = await _roleManager.AddClaimAsync(role, claim);

                if (!addResult.Succeeded)
                {
                    throw new InternalServerException("Add permission failed.", GetErrors(addResult));
                }
            }

            await _events.PublishAsync(new ApplicationRoleUpdatedEvent(role.Id, role.Name!, true));

            return "Permission Added.";
        }

        public async Task<string> RemovePermissionFromRoleAsync(string roleId, int permissionToRemove)
        {
            // Retrieve the role
            var role = await _roleManager.FindByIdAsync(roleId) ?? throw new NotFoundException("Role Not Found");

            var permission = await GetByIdAsync(permissionToRemove) ?? throw new NotFoundException("Permission Not Found");

            if (permission.ClaimValue != null)
            {
                var claim = new Claim(LocalAppClaims.Permission, permission.ClaimValue);
                var removeResult = await _roleManager.RemoveClaimAsync(role, claim);

                if (!removeResult.Succeeded)
                {
                    throw new InternalServerException("Remove permission failed.", GetErrors(removeResult));
                }
            }

            await _events.PublishAsync(new ApplicationRoleUpdatedEvent(role.Id, role.Name!, true));

            return "Permission Removed.";
        }

        public static List<string> GetErrors(IdentityResult result) =>
            result.Errors.Select(e => e.Description.ToString()).ToList();
    }
}
