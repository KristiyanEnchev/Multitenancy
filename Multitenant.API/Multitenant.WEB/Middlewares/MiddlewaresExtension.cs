namespace Multitenant.WEB.Middlewares
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using Multitenant.Models.Middleware;
    using Multitenant.WEB.Middlewares.Logging;
    using Multitenant.WEB.Middlewares.Exception;
    using Multitenant.WEB.Middlewares.CurrentUser;
    using Multitenant.Application.Interfaces.Utility.User;

    internal static class MiddlewaresExtension
    {
        internal static IApplicationBuilder UseCurrentUser(this IApplicationBuilder app) =>
            app.UseMiddleware<CurrentUserMiddleware>();

        internal static IServiceCollection AddCurrentUser(this IServiceCollection services) =>
            services
                .AddScoped<CurrentUserMiddleware>()
                .AddScoped<ICurrentUser, Multitenant.WEB.Services.CurrentUser>()
                .AddScoped(sp => (ICurrentUserInitializer)sp.GetRequiredService<ICurrentUser>());

        internal static IServiceCollection AddExceptionMiddleware(this IServiceCollection services) =>
            services.AddScoped<ExceptionMiddleware>();

        internal static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder app) =>
            app.UseMiddleware<ExceptionMiddleware>();

        internal static IServiceCollection AddRequestLogging(this IServiceCollection services, IConfiguration config)
        {
            if (GetMiddlewareSettings(config).EnableHttpsLogging)
            {
                services.AddSingleton<RequestLoggingMiddleware>();
                services.AddScoped<ResponseLoggingMiddleware>();
            }

            return services;
        }

        internal static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app, IConfiguration config)
        {
            if (GetMiddlewareSettings(config).EnableHttpsLogging)
            {
                app.UseMiddleware<RequestLoggingMiddleware>();
                app.UseMiddleware<ResponseLoggingMiddleware>();
            }

            return app;
        }

        private static MiddlewareSettings GetMiddlewareSettings(IConfiguration configuration)
        {
            MiddlewareSettings settings = configuration.GetSection(nameof(MiddlewareSettings)).Get<MiddlewareSettings>()!;

            return settings;
        }
    }
}