namespace Multitenant.WEB.Extensions.Permissions
{
    using Microsoft.AspNetCore.Authorization;

    using Multitenant.Shared.ClaimsPrincipal;
    using Multitenant.Application.Interfaces.Identity;

    internal class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IUserService _userService;

        public PermissionAuthorizationHandler(IUserService userService) =>
            _userService = userService;

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.User?.GetUserId() is { } userId &&
                await _userService.HasPermissionAsync(userId, requirement.Permission))
            {
                context.Succeed(requirement);
            }
            else if (context.User?.GetUserId() is { } userId1 &&
                await _userService.IsInRoleAsync(userId1, requirement.Permission))
            {
                context.Succeed(requirement);
            }
        }
    }
}