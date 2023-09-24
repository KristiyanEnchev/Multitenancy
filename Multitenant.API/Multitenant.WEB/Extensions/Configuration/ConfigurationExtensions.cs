namespace Multitenant.WEB.Extensions.Configuration
{
    using Microsoft.Extensions.Configuration;
    using Multitenant.Models.Persistence;

    public static class ConfigurationExtensions
    {
        public static string GetDbConnectionString(this IConfiguration configuration)
        {
            var settings = configuration.GetSection(nameof(DatabaseSettings)).Get<DatabaseSettings>();
            return settings?.ConnectionString;
        }

        public static string GetTenantConnectionString(this IConfiguration configuration)
        {
            var settings = configuration.GetSection(nameof(DatabaseSettings)).Get<DatabaseSettings>();
            return settings?.ConnectionString;
        }

        public static string GetRedisConnectionString(this IConfiguration configuration)
        {
            var settings = configuration.GetSection(nameof(RedisSettings)).Get<RedisSettings>();
            return settings?.RedisURL;
        }
    }
}
