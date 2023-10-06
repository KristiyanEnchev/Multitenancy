namespace Multitenant.Application.Interfaces.Identity
{
    using Multitenant.Models.Identity;
    using Multitenant.Application.Identity.Role;
    using Multitenant.Application.Interfaces.DependencyScope;

    public interface IRoleService : ITransientService
    {
        Task<List<RoleDto>> GetListAsync(CancellationToken cancellationToken);

        Task<int> GetCountAsync(CancellationToken cancellationToken);

        Task<bool> ExistsAsync(string roleName, string? excludeId);

        Task<RoleDto> GetByIdAsync(string id);

        Task<RoleDto> GetByIdWithPermissionsAsync(string roleId, CancellationToken cancellationToken);
        Task<RoleDto> GetByNameWithPermissionsAsync(string roleName, CancellationToken cancellationToken);

        Task<string> CreateOrUpdateAsync(CreateOrUpdateRoleRequest request);

        Task<string> UpdatePermissionsAsync(UpdateRolePermissionsRequest request, CancellationToken cancellationToken);

        Task<string> DeleteAsync(string id);
    }
}