namespace Multitenant.Application.Interfaces.Utility
{
    using System.Security.Claims;

    public interface ICurrentUserInitializer
    {
        void SetCurrentUser(ClaimsPrincipal user);

        void SetCurrentUserId(string userId);
    }
}