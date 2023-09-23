namespace Multitenant.WEB.Extensions.Permissions
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.DependencyInjection;

    internal static class PermissionsExtension
    {
        public static IServiceCollection AddPermissions(this IServiceCollection services)
        {
            return services
            .AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            //.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        }
    }
}