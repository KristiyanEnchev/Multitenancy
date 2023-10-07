namespace Multitenant.Application.Interfaces.Identity
{
    using Multitenant.Models.Permission;
    using Multitenant.Application.Interfaces.DependencyScope;
    using Multitenant.Application.Identity.Permission;

    public interface IPermissionService : ITransientService
    {
        Task<List<PermissionDto>> GetListAsync(CancellationToken cancellationToken);
        Task<PermissionDto> GetByIdAsync(int id);
        Task<PermissionDto> GetByNameAsync(string name);
        Task<bool> ExistsAsync(string permissionName, int? excludeId);
        Task<bool> ExistsByIdAsync(int permissionId, int? excludeId);
        Task<bool> RoleHasPermission(string roleId, int permissionId);
        Task<int> GetCountAsync(CancellationToken cancellationToken);
        Task<string> AddPermissionToRoleAsync(string roleId, int permissionToAdd);
        Task<string> RemovePermissionFromRoleAsync(string roleId, int permissionToRemove);
        Task<string> UpdatePermissionsAsync(UpdateRolePermissionsRequest request, CancellationToken cancellationToken);
    }
}
