namespace Multitenant.WEB.Extensions.Healtchecks
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using Multitenant.Models.HealthCheck;
    using Multitenant.WEB.Extensions.Configuration;

    internal static class HealtExtension
    {
        internal static IServiceCollection AddHealth(this IServiceCollection services, IConfiguration configuration)
        {
            var healtSettings = configuration.GetSection(nameof(Health)).Get<Health>();

            services.AddSingleton<CustomHealthCheckResponseWriter>();

            var tenantHealthChecks = healtSettings?.TenantHealthChecks;
            var databaseHealthChecks = healtSettings?.DatabaseHealthChecks;
            var memoryCacheHealthChecks = healtSettings?.MemoryCacheHealthChecks;

            var healthChecks = services.AddHealthChecks();

            healthChecks.AddCheck<TenantHealthCheck>("Tenant");

            if (tenantHealthChecks != null && (bool)tenantHealthChecks)
            {
                healthChecks
                    .AddSqlServer(configuration.GetTenantConnectionString(), name: "Tenant Database");
            }

            if (databaseHealthChecks != null && (bool)databaseHealthChecks)
            {
                healthChecks
                    .AddSqlServer(configuration.GetDbConnectionString(), name: "Identity Database");
            }

            if (memoryCacheHealthChecks != null && (bool)memoryCacheHealthChecks)
            {
                healthChecks
                    .AddRedis(configuration.GetRedisConnectionString());
            }

            return services;
        }

        internal static IEndpointConventionBuilder MapHealthCheck(this IEndpointRouteBuilder endpoints) =>
            endpoints.MapHealthChecks("/api/health", new HealthCheckOptions
            {
                ResponseWriter = (httpContext, result) => CustomHealthCheckResponseWriter.WriteResponse(httpContext, result),
            });
    }
}