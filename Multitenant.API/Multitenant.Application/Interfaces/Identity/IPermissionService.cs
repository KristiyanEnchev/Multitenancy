namespace Multitenant.Application.Interfaces.Identity
{
    using Multitenant.Models.Permission;
    using Multitenant.Application.Interfaces.DependencyScope;
    using Multitenant.Application.Identity.Permission;
    using Multitenant.Application.Identity.Role;

    public interface IPermissionService : ITransientService
    {
        Task<List<PermissionDto>> GetListAsync(CancellationToken cancellationToken);
        Task<PermissionDto> GetByIdAsync(int id);
        Task<List<PermissionDto>> GetByNameAsync(string name);
        Task<bool> ExistsAsync(string permissionName, int? excludeId);
        Task<bool> ExistsByIdAsync(int permissionId, int? excludeId);
        Task<bool> RoleHasPermission(string roleId, int permissionId);
        Task<int> GetCountAsync(CancellationToken cancellationToken);
        Task<string> AddPermissionToRoleAsync(AddPermissionToRoleRequest request);
        Task<string> RemovePermissionFromRoleAsync(RemovePermissionFromRoleRequest request);
        Task<string> UpdatePermissionsAsync(UpdateRolePermissionsRequest request, CancellationToken cancellationToken);
    }
}
