namespace Multitenant.WEB.Extensions.Authentication
{
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.AspNetCore.Authentication.JwtBearer;

    using Multitenant.Models.Security;

    internal static class AuthenticationExtension
    {
        internal static IServiceCollection AddJWTAuthentiation(this IServiceCollection services)
        {
            services.AddOptions<JwtSettings>()
                .BindConfiguration($"SecuritySettings:{nameof(JwtSettings)}")
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();

            return services
                .AddAuthentication(authentication =>
                {
                    authentication.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    authentication.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, null!)
                .Services;
        }
    }
}
