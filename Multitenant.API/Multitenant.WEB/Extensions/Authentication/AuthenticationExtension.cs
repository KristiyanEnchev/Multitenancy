namespace Multitenant.WEB.Extensions.Authentication
{
    using Microsoft.Identity.Web;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.AspNetCore.Authentication.JwtBearer;

    using Serilog;

    using Multitenant.Models.Security;
    using Multitenant.WEB.Extensions.Permissions;
    using Multitenant.WEB.Extensions.Authentication.AzureAd;

    internal static class AuthenticationExtension
    {
        internal static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration config)
        {
            services.AddPermissions();

            services.Configure<SecuritySettings>(config.GetSection(nameof(SecuritySettings)));
            return config["SecuritySettings:Provider"]!.Equals("AzureAd", StringComparison.OrdinalIgnoreCase)
                ? services.AddAzureAdAuth(config)
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

        internal static IServiceCollection AddAzureAdAuth(this IServiceCollection services, IConfiguration config)
        {
            var logger = Log.ForContext(typeof(AzureAdJwtBearerEvents));

            services
                .AddAuthorization()
                .AddAuthentication(authentication =>
                {
                    authentication.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    authentication.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddMicrosoftIdentityWebApi(
                    jwtOptions => jwtOptions.Events = new AzureAdJwtBearerEvents(logger, config),
                    msIdentityOptions => config.GetSection("SecuritySettings:AzureAd").Bind(msIdentityOptions));

            return services;
        }
    }
}
