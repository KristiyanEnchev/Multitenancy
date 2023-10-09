namespace Multitenant.WEB.Controllers.Personal
{
    using Microsoft.AspNetCore.Mvc;

    using Swashbuckle.AspNetCore.Annotations;

    using Multitenant.Models.Auditing;
    using Multitenant.Models.Identity;
    using Multitenant.Shared.ClaimsPrincipal;
    using Multitenant.Application.Persistence;
    using Multitenant.Application.Interfaces.Identity;
    using Multitenant.Application.Identity.UserIdentity;
    using Multitenant.Application.Identity.UserIdentity.Password;
    using Multitenant.WEB.Attributes;

    public class PersonalController : VersionNeutralApiController
    {
        private readonly IUserService _userService;

        public PersonalController(IUserService userService) => _userService = userService;

        [HttpGet("profile")]
        [SwaggerOperation("Get profile details of currently logged in user.", "")]
        public async Task<ActionResult<UserDetailsDto>> GetProfileAsync(CancellationToken cancellationToken)
        {
            return User.GetUserId() is not { } userId || string.IsNullOrEmpty(userId)
                ? Unauthorized()
                : Ok(await _userService.GetAsync(userId, cancellationToken));
        }

        [HttpPut("profile")]
        [SwaggerOperation("Update profile details of currently logged in user.", "")]
        public async Task<ActionResult> UpdateProfileAsync(UpdateUserRequest request)
        {
            if (User.GetUserId() is not { } userId || string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            await _userService.UpdateAsync(request, userId);
            return Ok();
        }

        [TenantIdHeader]
        [HttpPut("change-password")]
        [SwaggerOperation("Change password of currently logged in user.", "")]
        //[ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Register))]
        public async Task<ActionResult> ChangePasswordAsync(ChangePasswordRequest model)
        {
            if (User.GetUserId() is not { } userId || string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            model.UserId = userId;
            await Mediator.Send(model);
            return Ok();
        }

        [HttpGet("permissions")]
        [SwaggerOperation("Get permissions of currently logged in user.", "")]
        public async Task<ActionResult<List<string>>> GetPermissionsAsync(CancellationToken cancellationToken)
        {
            return User.GetUserId() is not { } userId || string.IsNullOrEmpty(userId)
                ? Unauthorized()
                : Ok(await _userService.GetPermissionsAsync(userId, cancellationToken));
        }

        [HttpGet("logs")]
        [SwaggerOperation("Get audit logs of currently logged in user.", "")]
        public Task<List<AuditDto>> GetLogsAsync()
        {
            return Mediator.Send(new GetMyAuditLogsRequest());
        }

    }
}
