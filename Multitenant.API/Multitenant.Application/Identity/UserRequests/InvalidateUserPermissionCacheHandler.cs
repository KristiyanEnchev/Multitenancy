namespace Multitenant.Application.Identity.UserRequests
{
    using Microsoft.AspNetCore.Identity;

    using Multitenant.Domain.Events;
    using Multitenant.Application.Events;
    using Multitenant.Application.Interfaces.Identity;
    using Multitenant.Domain.Entities.Identity;

    internal class InvalidateUserPermissionCacheHandler :
    IEventNotificationHandler<ApplicationUserUpdatedEvent>,
    IEventNotificationHandler<ApplicationRoleUpdatedEvent>
    {
        private readonly IUserService _userService;
        private readonly UserManager<User> _userManager;

        public InvalidateUserPermissionCacheHandler(IUserService userService, UserManager<User> userManager) =>
            (_userService, _userManager) = (userService, userManager);

        public async Task Handle(EventNotification<ApplicationUserUpdatedEvent> notification, CancellationToken cancellationToken)
        {
            if (notification.Event.RolesUpdated)
            {
                await _userService.InvalidatePermissionCacheAsync(notification.Event.UserId, cancellationToken);
            }
        }

        public async Task Handle(EventNotification<ApplicationRoleUpdatedEvent> notification, CancellationToken cancellationToken)
        {
            if (notification.Event.PermissionsUpdated)
            {
                foreach (var user in await _userManager.GetUsersInRoleAsync(notification.Event.RoleName))
                {
                    await _userService.InvalidatePermissionCacheAsync(user.Id, cancellationToken);
                }
            }
        }
    }
}