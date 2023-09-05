namespace Multitenant.WEB
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using Multitenant.WEB.Extensions.Swagger;
    using Multitenant.WEB.Healtchecks;
    using Multitenant.WEB.Middlewares;

    public static class Startup
    {
        public static IServiceCollection AddWEB(this IServiceCollection services, IConfiguration config)
        {
            services.AddEndpointsApiExplorer();

            return services
                .AddExceptionMiddleware()
                .AddCurrentUser()
                .AddHealthCheck()
                .AddSwaggerDocumentation(config)
                .AddRequestLogging(config)
                .AddRouting(options => options.LowercaseUrls = true);
        }

        private static IServiceCollection AddHealthCheck(this IServiceCollection services) =>
            services.AddHealthChecks().AddCheck<TenantHealthCheck>("Tenant").Services;

        public static IApplicationBuilder UseWEB(this IApplicationBuilder builder, IConfiguration config) =>
            builder
                .UseExceptionMiddleware()
                .UseRouting()
                .UseAuthentication()
                .UseCurrentUser()
                .UseAuthorization()
                .UseRequestLogging(config)
                .UseSwaggerDocumentation(config);

        public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder)
        {
            builder.MapControllers().RequireAuthorization();
            builder.MapHealthCheck();
            return builder;
        }

        private static IEndpointConventionBuilder MapHealthCheck(this IEndpointRouteBuilder endpoints) =>
            endpoints.MapHealthChecks("/api/health");
    }
}
