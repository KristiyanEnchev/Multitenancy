namespace Multitenant.Application.Interfaces.Utility.User
{
    using System.Security.Claims;

    public interface ICurrentUserInitializer
    {
        void SetCurrentUser(ClaimsPrincipal user);

        void SetCurrentUserId(string userId);
    }
}