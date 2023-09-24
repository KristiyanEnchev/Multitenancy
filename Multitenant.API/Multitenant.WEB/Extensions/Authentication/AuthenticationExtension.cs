namespace Multitenant.WEB.Extensions.Authentication
{
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.AspNetCore.Authentication.JwtBearer;

    using Multitenant.Models.Security;
    using Multitenant.WEB.Extensions.Permissions;

    internal static class AuthenticationExtension
    {
        internal static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration config)
        {
            services.AddPermissions();

            services.Configure<SecuritySettings>(config.GetSection(nameof(SecuritySettings)));
            return config["SecuritySettings:Provider"]!.Equals("AzureAd", StringComparison.OrdinalIgnoreCase)
                ? services
                : services.AddJWTAuthentiation();
        }

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
