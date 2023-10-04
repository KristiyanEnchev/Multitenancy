namespace Multitenant.WEB.Controllers.User
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;

    using Multitenant.Models.Identity;
    using Multitenant.WEB.Attributes;
    using Multitenant.WEB.Extensions.Permissions;
    using Multitenant.Application.Interfaces.Identity;
    using Multitenant.Application.Identity.UserIdentity;
    using Multitenant.Application.Identity.UserIdentity.Password;
    using Multitenant.Shared.ClaimsPrincipal;
    using Swashbuckle.AspNetCore.Annotations;
    using System.ComponentModel.DataAnnotations;

    public class UsersController : VersionNeutralApiController
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService) => _userService = userService;

        [HttpGet]
        [MustHavePermission(Action.View, Resource.Users)]
        [SwaggerOperation("Get list of all users.", "")]
        public Task<List<UserDetailsDto>> GetListAsync(CancellationToken cancellationToken)
        {
            return _userService.GetListAsync(cancellationToken);
        }

        [HttpGet("{id}")]
        [MustHavePermission(Action.View, Resource.Users)]
        [SwaggerOperation("Get a user's details.", "")]
        public Task<UserDetailsDto> GetByIdAsync(string id, CancellationToken cancellationToken)
        {
            return _userService.GetAsync(id, cancellationToken);
        }

        [HttpPost]
        [MustHavePermission(Action.Create, Resource.Users)]
        [SwaggerOperation("Creates a new user.", "")]
        public Task<string> CreateAsync(CreateUserRequest request)
        {
            // TODO: check if registering anonymous users is actually allowed (should probably be an appsetting)
            // and return UnAuthorized when it isn't
            // Also: add other protection to prevent automatic posting (captcha?)
            return _userService.CreateAsync(request, GetOriginFromRequest());
        }

        [HttpPut(nameof(Update) + "/{id}")]
        public async Task<ActionResult<BaseResponse>> Update([Required] string id, UpdateUserModel model)
        {
            return await userService.Update(id, model);
        }

        [HttpDelete(nameof(Delete) + "/{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            return await userService.Delete(id).ToActionResult();
        }

        [HttpGet("{id}/roles")]
        [MustHavePermission(Action.View, Resource.UserRoles)]
        [SwaggerOperation("Get a user's roles.", "")]
        public Task<List<UserRoleDto>> GetRolesAsync(string id, CancellationToken cancellationToken)
        {
            return _userService.GetRolesAsync(id, cancellationToken);
        }

        [HttpPost("{id}/roles")]
        //[ApiConventionMethod(typeof(ApiConventions), nameof(ApiConventions.Register))]
        [MustHavePermission(Action.Update, Resource.UserRoles)]
        [SwaggerOperation("Update a user's assigned roles.", "")]
        public Task<string> AssignRolesAsync(string id, UserRolesRequest request, CancellationToken cancellationToken)
        {
            return _userService.AssignRolesAsync(id, request, cancellationToken);
        }

        [HttpPost("{id}/toggle-status")]
        [MustHavePermission(Action.Update, Resource.Users)]
        //[ApiConventionMethod(typeof(ApiConventions), nameof(ApiConventions.Register))]
        [SwaggerOperation("Toggle a user's active status.", "")]
        public async Task<ActionResult> ToggleStatusAsync(string id, ToggleUserStatusRequest request, CancellationToken cancellationToken)
        {
            if (id != request.UserId)
            {
                return BadRequest();
            }

            await _userService.ToggleStatusAsync(request, cancellationToken);
            return Ok();
        }


        private string GetOriginFromRequest() => $"{Request.Scheme}://{Request.Host.Value}{Request.PathBase.Value}";
    }

}
