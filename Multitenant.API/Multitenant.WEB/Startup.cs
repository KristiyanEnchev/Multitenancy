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
            services.AddControllers();
            services
                .AddSwaggerDocumentation(config)
                .AddExceptionMiddleware()
                .AddCurrentUser()
                .AddHealthCheck()
                .AddRequestLogging(config)
                .AddRouting(options => options.LowercaseUrls = true);

            return services;
        }

        public static IApplicationBuilder UseWEB(this IApplicationBuilder builder, IConfiguration config)
        {
            return builder
                .UseHttpsRedirection()
                .UseSwaggerDocumentation(config)
                .UseExceptionMiddleware()
                .UseRouting()
                .UseAuthentication()
                .UseCurrentUser()
                .UseAuthorization()
                .UseRequestLogging(config);
        }

        public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder)
        {
            builder.MapDefaultControllerRoute();
            builder.MapControllers().RequireAuthorization();
            builder.MapHealthCheck();
            return builder;
        }

        private static IServiceCollection AddHealthCheck(this IServiceCollection services) =>
            services.AddHealthChecks().AddCheck<TenantHealthCheck>("Tenant").Services;

        private static IEndpointConventionBuilder MapHealthCheck(this IEndpointRouteBuilder endpoints) =>
            endpoints.MapHealthChecks("/api/health");
    }
}
