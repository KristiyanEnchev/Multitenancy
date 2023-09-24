namespace Multitenant.Infrastructure.Services.Cache
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using Multitenant.Models.Cache;
    using Multitenant.Application.Interfaces.Cache;

    public static class CacheExtension
    {
        internal static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration config)
        {
            var settings = config.GetSection(nameof(CacheSettings)).Get<CacheSettings>();
            if (settings == null) return services;
            if (settings.UseDistributedCache)
            {
                if (settings.PreferRedis)
                {
                    services.AddStackExchangeRedisCache(options =>
                    {
                        options.Configuration = settings.RedisURL;
                        options.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions()
                        {
                            AbortOnConnectFail = true,
                            EndPoints = { settings.RedisURL }
                        };
                    });
                }
                else
                {
                    services.AddDistributedMemoryCache();
                }

                services.AddTransient<ICacheService, DistributedCacheService>();
            }
            else
            {
                services.AddTransient<ICacheService, LocalCacheService>();
            }

            services.AddMemoryCache();
            return services;
        }
    }
}
