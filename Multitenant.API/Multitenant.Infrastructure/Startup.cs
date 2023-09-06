namespace Multitenant.Infrastructure
{
    using System.Reflection;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using ElmahCore.Mvc;

    using MediatR;

    using Multitenant.Infrastructure.Extensions.Cors;
    using Multitenant.Infrastructure.Extensions.Elmah;
    using Multitenant.Infrastructure.Extensions.SecurityHeaders;
    using Multitenant.Infrastructure.Extensions.Validations;
    using Multitenant.Infrastructure.Extensions.Versioning;
    using Multitenant.Infrastructure.Services.Helpers;

    public static class Startup
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            var applicationAssembly = typeof(Multitenant.Application.Startup).GetTypeInfo().Assembly;
            //MapsterSettings.Configure();
            return services
                .AddVersioning()
                //.AddAuth(config)
                //.AddBackgroundJobs(config)
                //.AddCaching(config)
                .AddCorsPolicy(config)
                .AddBehaviours(applicationAssembly)
                //.AddHealthCheck()
                .AddMediatR(Assembly.GetExecutingAssembly())
                //.AddMultitenancy()
                //.AddNotifications(config)
                .AddElmahConfig(config)
                //.AddPersistence()
                .AddServices();

        }

        //public static async Task InitializeDatabasesAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
        //{
        //    // Create a new scope to retrieve scoped services
        //    using var scope = services.CreateScope();

        //    await scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>()
        //        .InitializeDatabasesAsync(cancellationToken);
        //}

        public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder builder, IConfiguration config) =>
            builder
                .UseStaticFiles()
                .UseSecurityHeaders(config)
                //.UseFileStorage()
                .UseCorsPolicy()
                //.UseMultiTenancy()
                .UseElmah();
        //.UseHangfireDashboard(config);
    }
}