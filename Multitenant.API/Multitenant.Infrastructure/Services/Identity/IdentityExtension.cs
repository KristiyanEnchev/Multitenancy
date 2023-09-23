namespace Multitenant.Infrastructure.Services.Identity
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;

    using Multitenant.Domain.Entities.Identity;
    using Multitenant.Infrastructure.Services.Tenant.Context;

    internal static class IdentityExtension
    {
        internal static IServiceCollection AddIdentity(this IServiceCollection services) =>
         services
            .AddIdentity<User, UserRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .Services;
    }
}