namespace Multitenant.WEB.Controllers.Identity
{
    using Microsoft.AspNetCore.Mvc;

    using Swashbuckle.AspNetCore.Annotations;

    using Multitenant.Application.Identity.Role;
    using Multitenant.Application.Interfaces.Identity;
    using Multitenant.Models.Identity;
    using Multitenant.Shared.ClaimsPrincipal;
    using Multitenant.WEB.Extensions.Permissions;

    public class RolesController : VersionNeutralApiController
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService) => _roleService = roleService;

        [HttpGet]
        //[Authorize(Roles.Admin)]
        [MustHavePermission(Action.View, Resource.Roles)]
        [SwaggerOperation("Get a list of all roles.", "")]
        public Task<List<RoleDto>> GetListAsync(CancellationToken cancellationToken)
        {
            return _roleService.GetListAsync(cancellationToken);
        }

        [HttpGet("roleId")]
        [MustHavePermission(Action.View, Resource.Roles)]
        [SwaggerOperation("Get role details.", "")]
        public Task<RoleDto> GetByIdAsync([FromQuery] string roleId)
        {
            return _roleService.GetByIdAsync(roleId);
        }

        [HttpGet("roleName")]
        [MustHavePermission(Action.View, Resource.Roles)]
        [SwaggerOperation("Get role details.", "")]
        public Task<RoleDto> GetByNameAsync([FromQuery] string roleName, CancellationToken cancellationToken)
        {
            return _roleService.GetByNameWithPermissionsAsync(roleName, cancellationToken);
        }

        [HttpGet("permissions")]
        [MustHavePermission(Action.View, Resource.RoleClaims)]
        [SwaggerOperation("Get role details with its permissions.", "")]
        public Task<RoleDto> GetByIdWithPermissionsAsync([FromQuery] string roleId, CancellationToken cancellationToken)
        {
            return _roleService.GetByIdWithPermissionsAsync(roleId, cancellationToken);
        }

        [HttpPost]
        [MustHavePermission(Action.Create, Resource.Roles)]
        [SwaggerOperation("Create a role.", "")]
        public Task<string> CreaterRoleAsync(CreateOrUpdateRoleRequest request)
        {
            return _roleService.CreateOrUpdateAsync(request);
        }

        [HttpPut]
        [MustHavePermission(Action.Create, Resource.Roles)]
        [SwaggerOperation("Update a role.", "")]
        public Task<string> UpdateRoleAsync(CreateOrUpdateRoleRequest request)
        {
            return _roleService.CreateOrUpdateAsync(request);
        }

        [HttpDelete]
        [MustHavePermission(Action.Delete, Resource.Roles)]
        [SwaggerOperation("Delete a role.", "")]
        public Task<string> DeleteAsync([FromQuery] string id)
        {
            return _roleService.DeleteAsync(id);
        }
    }
}
