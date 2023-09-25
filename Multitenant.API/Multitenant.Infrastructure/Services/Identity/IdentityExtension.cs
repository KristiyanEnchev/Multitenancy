namespace Multitenant.Infrastructure.Services.Identity
{
    using Microsoft.Identity.Web;
    using Microsoft.Extensions.Options;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.AspNetCore.Authentication.JwtBearer;

    using Serilog;

    using Multitenant.Models.Security;
    using Multitenant.Domain.Entities.Identity;
    using Multitenant.Infrastructure.Services.Identity.AzureAd;
    using Multitenant.Infrastructure.Services.Tenant.Context;
    using Multitenant.Infrastructure.Extensions.Authentication;

    internal static class IdentityExtension
    {
        internal static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration config)
        {
            services.AddIdentity();
            services.Configure<SecuritySettings>(config.GetSection(nameof(SecuritySettings)));
            return config["SecuritySettings:Provider"]!.Equals("AzureAd", StringComparison.OrdinalIgnoreCase)
                ? services.AddAzureAdAuth(config)
                : services.AddJWTAuthentiation();
        }

        internal static IServiceCollection AddIdentity(this IServiceCollection services) =>
         services
            .AddIdentity<User, UserRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddTokenProvider("MultiAuth", typeof(DataProtectorTokenProvider<User>))
            .Services;

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