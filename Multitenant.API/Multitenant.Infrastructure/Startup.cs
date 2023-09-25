namespace Multitenant.Infrastructure
{
    using System.Reflection;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using ElmahCore.Mvc;

    using Multitenant.Application;
    using Multitenant.Application.Interfaces.Persistance;
    using Multitenant.Infrastructure.Extensions.Cors;
    using Multitenant.Infrastructure.Extensions.Elmah;
    using Multitenant.Infrastructure.Extensions.SecurityHeaders;
    using Multitenant.Infrastructure.Extensions.Validations;
    using Multitenant.Infrastructure.Extensions.Versioning;
    using Multitenant.Infrastructure.Extensions.Service;
    using Multitenant.Infrastructure.Extensions.Mapping;
    using Multitenant.Infrastructure.Services.Persistence;
    using Multitenant.Infrastructure.Extensions.Tenant;
    using Multitenant.Infrastructure.Services.Identity;
    using Multitenant.Infrastructure.Services.Cache;

    public static class Startup
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            var applicationAssembly = typeof(Multitenant.Application.Startup).GetTypeInfo().Assembly;
            services.AddApplication();

            services.AddHttpClient();
            MapsterSettings.Configure();
            return services
                .AddVersioning()
                .AddCaching(config)
                .AddCorsPolicy(config)
                .AddBehaviours(applicationAssembly)
                .AddMultitenancy()
                //.AddNotifications(config)
                .AddElmahConfig(config)
                .AddPersistence()
                //.AddIdentity()
                .AddAuth(config)
                .AddServices();


        }

        public static async Task InitializeDatabasesAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
        {
            // Create a new scope to retrieve scoped services
            using var scope = services.CreateScope();

            await scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>()
                .InitializeDatabasesAsync(cancellationToken);
        }

        public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder builder, IConfiguration config) =>
            builder
                .UseStaticFiles()
                .UseSecurityHeaders(config)
                .UseCorsPolicy()
                .UseMultiTenancy()
                .UseElmah();
    }
}