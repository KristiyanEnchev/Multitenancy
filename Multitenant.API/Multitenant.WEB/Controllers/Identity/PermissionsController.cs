namespace Multitenant.WEB.Controllers.Identity
{
    using Microsoft.AspNetCore.Mvc;

    using Swashbuckle.AspNetCore.Annotations;

    using Multitenant.Models.Permission;
    using Multitenant.Shared.ClaimsPrincipal;
    using Multitenant.WEB.Extensions.Permissions;
    using Multitenant.Application.Interfaces.Identity;
    using Multitenant.Application.Identity.Permission;
    using Multitenant.Application.Identity.Role;

    public class PermissionsController : VersionNeutralApiController
    {
        private readonly IPermissionService _permissionService;

        public PermissionsController(IPermissionService permissionService) => _permissionService = permissionService;

        [HttpGet]
        [MustHavePermission(Action.View, Resource.RoleClaims)]
        [SwaggerOperation("Get a list of all permissions.", "")]
        public Task<List<PermissionDto>> GetListAsync(CancellationToken cancellationToken)
        {
            return _permissionService.GetListAsync(cancellationToken);
        }

        [HttpGet("permissionId")]
        [MustHavePermission(Action.View, Resource.RoleClaims)]
        [SwaggerOperation("Get permission details.", "")]
        public Task<PermissionDto> GetByIdAsync([FromQuery] int permissionId)
        {
            return _permissionService.GetByIdAsync(permissionId);
        }

        [HttpGet("permissionName")]
        [MustHavePermission(Action.View, Resource.RoleClaims)]
        [SwaggerOperation("Get permission details.", "")]
        public async Task<List<PermissionDto>> GetByNameAsync([FromQuery] string permissionName)
        {
            return await _permissionService.GetByNameAsync(permissionName);
        }

        [HttpPatch("add")]
        [MustHavePermission(Action.Update, Resource.RoleClaims)]
        [SwaggerOperation("Add permission to a role.", "")]
        public async Task<string> AddPermissionToRoleAsync(AddPermissionToRoleRequest request)
        {
            return await Mediator.Send(request);
        }

        [HttpPatch("remove")]
        [MustHavePermission(Action.Update, Resource.RoleClaims)]
        [SwaggerOperation("Remove permission from a role.", "")]
        public async Task<ActionResult<string>> RemovePermissionFromRoleAsync(RemovePermissionFromRoleRequest request)
        {
            return await Mediator.Send(request);
            //return await _permissionService.RemovePermissionFromRoleAsync(request);
        }

        [HttpPut("{id}")]
        [MustHavePermission(Action.Update, Resource.RoleClaims)]
        [SwaggerOperation("Update a role's permissions.", "")]
        public async Task<ActionResult<string>> UpdatePermissionsAsync(string id, UpdateRolePermissionsRequest request, CancellationToken cancellationToken)
        {
            if (id != request.RoleId)
            {
                return BadRequest();
            }

            return Ok(await _permissionService.UpdatePermissionsAsync(request, cancellationToken));
        }
    }
}
