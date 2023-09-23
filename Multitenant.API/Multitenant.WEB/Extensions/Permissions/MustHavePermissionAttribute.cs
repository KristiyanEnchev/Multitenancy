namespace Multitenant.WEB.Extensions.Permissions
{
    using Microsoft.AspNetCore.Authorization;

    using Multitenant.Shared.ClaimsPrincipal;

    public class MustHavePermissionAttribute : AuthorizeAttribute
    {
        public MustHavePermissionAttribute(string action, string resource) =>
            Policy = Permission.NameFor(action, resource);
    }
}