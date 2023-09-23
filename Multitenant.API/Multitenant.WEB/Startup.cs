namespace Multitenant.WEB
{
    using System.Reflection;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using MediatR;

    using Multitenant.WEB.Extensions.Authentication;
    using Multitenant.WEB.Extensions.Healtchecks;
    using Multitenant.WEB.Extensions.Swagger;
    using Multitenant.WEB.Middlewares;
    using Multitenant.WEB.Extensions.Permissions;

    public static class Startup
    {
        public static IServiceCollection AddWEB(this IServiceCollection services, IConfiguration config)
        {
            services.AddHttpContextAccessor();
            services.AddControllers();
            services
                .AddAuth(config)
                .AddCurrentUser()
                .AddExceptionMiddleware()
                .AddMediatR(Assembly.GetExecutingAssembly())
                .AddHealth(config)
                .AddSwaggerDocumentation(config)
                .AddRequestLogging(config)
                .AddRouting(options => options.LowercaseUrls = true);
            return services;
        }

        public static IApplicationBuilder UseWEB(this IApplicationBuilder builder, IConfiguration config)
        {
            return builder
                .UseHttpsRedirection()
                .UseExceptionMiddleware()
                .UseRouting()
                .UseAuthentication()
                .UseCurrentUser()
                .UseAuthorization()
                .UseRequestLogging(config)
                .UseSwaggerDocumentation(config);
        }

        public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder)
        {
            builder.MapControllers().RequireAuthorization();
            builder.MapHealthCheck();
            return builder;
        }
    }
}
