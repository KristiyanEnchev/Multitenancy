using ElmahCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Multitenant.Application.Interfaces.Persistance;
using Multitenant.Application.Interfaces.Utility.User;
using Multitenant.Infrastructure.Extensions.Cors;
using Multitenant.Infrastructure.Extensions.Elmah;
using Multitenant.Infrastructure.Extensions.SecurityHeaders;
using Multitenant.Infrastructure.Extensions.Validations;
using Multitenant.Infrastructure.Services.Helpers;
using Multitenant.Infrastructure.Services.Utility;
using System.Reflection;

namespace Multitenant.Infrastructure
{
    public static class Startup
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            var applicationAssembly = typeof(Multitenant.Application.Startup).GetTypeInfo().Assembly;
            //MapsterSettings.Configure();
            return services
                .AddApiVersioning()
                //.AddAuth(config)
                //.AddBackgroundJobs(config)
                //.AddCaching(config)
                .AddCorsPolicy(config)
                //.AddExceptionMiddleware()
                .AddBehaviours(applicationAssembly)
                //.AddHealthCheck()
                //.AddPOLocalization(config)
                //.AddMailing(config)
                .AddMediatR(Assembly.GetExecutingAssembly())
                //.AddMultitenancy()
                //.AddNotifications(config)
                //.AddOpenApiDocumentation(config)
                .AddElmahConfig(config)
                //.AddPersistence()
                //.AddRequestLogging(config)
                .AddRouting(options => options.LowercaseUrls = true)
                .AddCurrentUser()
                .AddServices();

        }

        private static IServiceCollection AddCurrentUser(this IServiceCollection services) =>
            services
            .AddScoped<ICurrentUser, CurrentUser>()
            .AddScoped(sp => (ICurrentUserInitializer)sp.GetRequiredService<ICurrentUser>());

        //private static IServiceCollection AddApiVersioning(this IServiceCollection services) =>
        //    services.AddApiVersioning(config =>
        //    {
        //        config.DefaultApiVersion = new ApiVersion(1, 0);
        //        config.AssumeDefaultVersionWhenUnspecified = true;
        //        config.ReportApiVersions = true;
        //    });

        //private static IServiceCollection AddHealthCheck(this IServiceCollection services) =>
        //    services.AddHealthChecks().AddCheck<TenantHealthCheck>("Tenant").Services;

        //public static async Task InitializeDatabasesAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
        //{
        //    // Create a new scope to retrieve scoped services
        //    using var scope = services.CreateScope();

        //    await scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>()
        //        .InitializeDatabasesAsync(cancellationToken);
        //}

        public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder builder, IConfiguration config) =>
            builder
                //.UseRequestLocalization()
                .UseStaticFiles()
                .UseSecurityHeaders(config)
                //.UseFileStorage()
                //.UseExceptionMiddleware()
                //.UseRouting()
                .UseCorsPolicy()
                //.UseAuthentication()
                //.UseCurrentUser()
                //.UseMultiTenancy()
                .UseElmah();
        //.UseAuthorization();
        //.UseRequestLogging(config)
        //.UseHangfireDashboard(config)
        //.UseOpenApiDocumentation(config);

        //public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder)
        //{
        //    builder.MapControllers().RequireAuthorization();
        //    builder.MapHealthCheck();
        //    //builder.MapNotifications();
        //    return builder;
        //}

        //private static IEndpointConventionBuilder MapHealthCheck(this IEndpointRouteBuilder endpoints) =>
        //    endpoints.MapHealthChecks("/api/health");
    }
}