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
    using Multitenant.Models.Mailing;
    using Multitenant.Infrastructure.Services.Tenant;

    public static class Startup
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            var applicationAssembly = typeof(Multitenant.Application.Startup).GetTypeInfo().Assembly;
            services.AddApplication();

            services.AddHostedService<ExpiryJobRunner>();

            services.Configure<MailingSettings>(config.GetSection(nameof(MailingSettings)));

            services.AddHttpClient();
            //services.AddHttpClient("HttpClientName", c => { }).ConfigurePrimaryHttpMessageHandler(() =>
            //{
            //    var handler = new HttpClientHandler();
            //    handler.ServerCertificateCustomValidationCallback =
            //    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            //    return handler;
            //});
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


        //         private void ConfigureHttpClientHandler(HttpClient httpClient)
        // {
        //     var httpClientHandler = httpClient.GetOrCreateHttpClientHandler();

        //     // Specify the thumbprint of the trusted certificate
        //     var thumbprint = "YOUR_CERTIFICATE_THUMBPRINT_HERE";

        //     httpClientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
        //     {
        //         // Check if the certificate thumbprint matches the expected thumbprint
        //         if (cert is X509Certificate2 certificate && certificate.Thumbprint == thumbprint)
        //         {
        //             return true; // The certificate is trusted
        //         }

        //         // Certificate validation failed
        //         return false;
        //     };
        // }
    }
}