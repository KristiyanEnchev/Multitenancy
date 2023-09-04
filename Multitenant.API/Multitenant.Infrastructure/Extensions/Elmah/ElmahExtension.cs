namespace Multitenant.Infrastructure.Extensions.Elmah
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using ElmahCore.Mvc;

    using Multitenant.Models.Elmah;

    public static class ElmahExtension
    {
        public static IServiceCollection AddElmahConfig(this IServiceCollection services, IConfiguration config)
        {
            var elmahSettings = config.GetSection(nameof(ElmahSettings)).Get<ElmahSettings>();

            services.AddElmah(options =>
            {
                options.ConnectionString = elmahSettings?.ElmahDb;
                options.SqlServerDatabaseTableName = elmahSettings?.ElmahTable;
                options.ApplicationName = "Multitenant.API";
            });

            return services;
        }
    }
}
