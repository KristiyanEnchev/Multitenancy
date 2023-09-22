namespace Multitenant.Infrastructure.Extensions.Tenant
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Primitives;
    using Microsoft.Extensions.DependencyInjection;

    using Multitenant.Shared;
    using Multitenant.Models.Persistence;
    using Multitenant.Shared.ClaimsPrincipal;
    using Multitenant.Application.Interfaces.Tenant;
    using Multitenant.Shared.Constants.Multitenancy;
    using Multitenant.Infrastructure.Services.Tenant;
    using Multitenant.Infrastructure.Services.Persistence;
    using Multitenant.Infrastructure.Services.Tenant.Context;

    internal static class MultitenancyExtension
    {
        internal static IServiceCollection AddMultitenancy(this IServiceCollection services)
        {
            return services
                .AddDbContext<TenantDbContext>((p, m) =>
                {
                    // TODO: We should probably add specific dbprovider/connectionstring setting for the tenantDb with a fallback to the main databasesettings
                    var databaseSettings = p.GetRequiredService<IOptions<DatabaseSettings>>().Value;
                    m.UseDatabase(databaseSettings.DBProvider, databaseSettings.TenantConnectionString);
                })
                .AddMultiTenant<MultiTenantInfo>()
                    .WithClaimStrategy(LocalAppClaims.Tenant)
                    .WithHeaderStrategy(MultitenancyConstants.TenantIdName)
                    .WithQueryStringStrategy(MultitenancyConstants.TenantIdName)
                    .WithEFCoreStore<TenantDbContext, MultiTenantInfo>()
                    .Services
                .AddScoped<ITenantService, TenantService>();
        }

        internal static IApplicationBuilder UseMultiTenancy(this IApplicationBuilder app)
        {
            return app.UseMultiTenant();
        }

        private static FinbuckleMultiTenantBuilder<MultiTenantInfo> WithQueryStringStrategy(this FinbuckleMultiTenantBuilder<MultiTenantInfo> builder, string queryStringKey) =>
            builder.WithDelegateStrategy(context =>
            {
                if (context is not HttpContext httpContext)
                {
                    return Task.FromResult((string?)null);
                }

                httpContext.Request.Query.TryGetValue(queryStringKey, out StringValues tenantIdParam);

                return Task.FromResult((string?)tenantIdParam.ToString());
            });

    }
}
