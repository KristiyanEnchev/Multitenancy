namespace Multitenant.WEB.Controllers.Identity
{
    using Microsoft.AspNetCore.Mvc;

    using Swashbuckle.AspNetCore.Annotations;

    using Multitenant.Models.Identity;
    using Multitenant.WEB.Extensions.Permissions;
    using Multitenant.Application.Interfaces.Identity;
    using Multitenant.Application.Identity.UserIdentity;
    using Multitenant.Shared.ClaimsPrincipal;
    using Multitenant.Application.Identity.UserIdentity.Password;
    using Multitenant.Application.Exceptions;

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

        [HttpPut("{id}")]
        [MustHavePermission(Action.Create, Resource.Users)]
        [SwaggerOperation("Upda user deatils.", "")]
        public async Task<string> Update(UpdateUserRequest request, string userId)
        {
            return await _userService.UpdateAsync(request, userId);
        }

        [HttpDelete(nameof(Delete))]
        public async Task<string> Delete([FromQuery] string id)
        {
            return await _userService.DeleteUserAsync(id);
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

        [HttpPut("change-password")]
        [MustHavePermission(Action.Update, Resource.Users)]
        [SwaggerOperation("Change or reset password of user by Id.", "")]
        public async Task<string> ChangePasswordAsync(ChangeUserPasswordRequest model)
        {
            return await Mediator.Send(model);
        }


        private string GetOriginFromRequest() => $"{Request.Scheme}://{Request.Host.Value}{Request.PathBase.Value}";
    }

}
