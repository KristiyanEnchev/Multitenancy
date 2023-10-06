namespace Multitenant.WEB.Controllers.Identity
{
    using Microsoft.AspNetCore.Mvc;

    using Swashbuckle.AspNetCore.Annotations;

    using Multitenant.Application.Identity.Role;
    using Multitenant.Shared.ClaimsPrincipal;
    using Multitenant.WEB.Extensions.Permissions;
    using Multitenant.Application.Interfaces.Identity;

    public class PermissionsController : VersionNeutralApiController
    {
        private readonly IRoleService _roleService;

        public PermissionsController(IRoleService roleService) => _roleService = roleService;

        [HttpPut("{id}/permissions")]
        [MustHavePermission(Action.Update, Resource.RoleClaims)]
        [SwaggerOperation("Update a role's permissions.", "")]
        public async Task<ActionResult<string>> UpdatePermissionsAsync(string id, UpdateRolePermissionsRequest request, CancellationToken cancellationToken)
        {
            if (id != request.RoleId)
            {
                return BadRequest();
            }

            return Ok(await _roleService.UpdatePermissionsAsync(request, cancellationToken));
        }
    }
}
