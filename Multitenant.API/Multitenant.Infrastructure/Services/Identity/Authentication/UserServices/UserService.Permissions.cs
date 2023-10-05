namespace Multitenant.Auth.UserServices
{
    using Microsoft.EntityFrameworkCore;

    using Multitenant.Application.Exceptions;
    using Multitenant.Shared.ClaimsPrincipal;
    using Multitenant.Infrastructure.Extensions.Cache;

    public partial class UserService
    {
        public async Task<List<string>> GetPermissionsAsync(string userId, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(userId);

            _ = user ?? throw new UnauthorizedException("Authentication Failed.");

            var userRoles = await _userManager.GetRolesAsync(user);
            var permissions = new List<string>();
            foreach (var role in await _roleManager.Roles
                .Where(r => userRoles.Contains(r.Name!))
                .ToListAsync(cancellationToken))
            {
                permissions.AddRange(await _db.RoleClaims
                    .Where(rc => rc.RoleId == role.Id && rc.ClaimType == LocalAppClaims.Permission)
                    .Select(rc => rc.ClaimValue!)
                    .ToListAsync(cancellationToken));
            }

            return permissions.Distinct().ToList();
        }

        public async Task<bool> HasPermissionAsync(string userId, string permission, CancellationToken cancellationToken)
        {
            var permissions = await _cache.GetOrSetAsync(
                _cacheKeys.GetCacheKey(LocalAppClaims.Permission, userId),
                () => GetPermissionsAsync(userId, cancellationToken),
                cancellationToken: cancellationToken);

            return permissions?.Contains(permission) ?? false;
        }

        public async Task<bool> IsInRoleAsync(string userId, string roleName) 
        {
            var user = await _userManager.FindByIdAsync(userId) ?? throw new NotFoundException("Current user not found");
            return await _userManager.IsInRoleAsync(user, roleName);
        }

        public Task InvalidatePermissionCacheAsync(string userId, CancellationToken cancellationToken) =>
            _cache.RemoveAsync(_cacheKeys.GetCacheKey(LocalAppClaims.Permission, userId), cancellationToken);
    }
}