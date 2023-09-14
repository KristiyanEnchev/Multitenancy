namespace Multitenant.Infrastructure.Services.Persistence
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.DependencyInjection;

    using Serilog;

    using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

    using Multitenant.Domain.Contracts;
    using Multitenant.Models.Persistence;
    using Multitenant.Shared.Persistance;
    using Multitenant.Application.Interfaces.Persistance;
    using Multitenant.Infrastructure.Extensions.Service;
    using Multitenant.Infrastructure.Services.Tenant.Context;
    //using Multitenant.Infrastructure.Services.Persistence.Repository;
    using Multitenant.Infrastructure.Services.Persistence.Initialization;
    using Multitenant.Infrastructure.Services.Persistence.ConnectionString;
    using System.Reflection;

    internal static class Startup
    {
        private static readonly ILogger _logger = Log.ForContext(typeof(Startup));

        internal static IServiceCollection AddPersistence(this IServiceCollection services)
        {
            services.AddOptions<DatabaseSettings>()
                .BindConfiguration(nameof(DatabaseSettings))
                .PostConfigure(databaseSettings =>
                {
                    _logger.Information("Current DB Provider: {dbProvider}", databaseSettings.DBProvider);
                })
                .ValidateDataAnnotations()
                .ValidateOnStart();

            //return services
            //    .AddDbContext<ApplicationDbContext>((p, m) =>
            //    {
            //        var databaseSettings = p.GetRequiredService<IOptions<DatabaseSettings>>().Value;
            //        m.UseDatabase(databaseSettings.DBProvider, databaseSettings.ConnectionString);
            //    })

            return services.AddTransient<IDatabaseInitializer, DatabaseInitializer>()
            //.AddTransient<AppInitializer>()
            //.AddTransient<ApplicationDbSeeder>()
            .AddServices(typeof(ICustomSeeder), ServiceLifetime.Transient)
            .AddTransient<CustomSeederRunner>()

            .AddTransient<IConnectionStringSecurer, ConnectionStringSecurer>()
            .AddTransient<IConnectionStringValidator, ConnectionStringValidator>();

            //.AddRepositories();
        }

        internal static DbContextOptionsBuilder UseDatabase(this DbContextOptionsBuilder builder, string dbProvider, string connectionString)
        {
            //Assembly migrationsAssembly = FindMigrationsAssembly();

            return dbProvider.ToLowerInvariant() switch
            {
                DbProviderKeys.Npgsql => builder.UseNpgsql(connectionString, e =>
                                     e.MigrationsAssembly("Migrators.PostgreSQL")),
                DbProviderKeys.SqlServer => builder.UseSqlServer(connectionString, e =>
                                     e.MigrationsAssembly("Migrators.MSSQL")),
                DbProviderKeys.MySql => builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), e =>
                                     e.MigrationsAssembly("Migrators.MySQL")
                                      .SchemaBehavior(MySqlSchemaBehavior.Ignore)),
                DbProviderKeys.Oracle => builder.UseOracle(connectionString, e =>
                                     e.MigrationsAssembly("Migrators.Oracle")),
                DbProviderKeys.SqLite => builder.UseSqlite(connectionString, e =>
                                     e.MigrationsAssembly("Migrators.SqLite")),
                _ => throw new InvalidOperationException($"DB Provider {dbProvider} is not supported."),
            };
        }

        private static Assembly FindMigrationsAssembly()
        {
            var callingAssembly = Assembly.GetCallingAssembly(); // This is your startup project assembly.
            var referencedAssemblies = callingAssembly.GetReferencedAssemblies();

            // You can define your own logic here to determine which assembly to use.
            // For example, you can look for an assembly name that matches a specific pattern.
            var migrationsAssemblyName = referencedAssemblies.FirstOrDefault(assemblyName =>
                assemblyName.Name.Equals("Migrators.MSSQL", StringComparison.OrdinalIgnoreCase));

            if (migrationsAssemblyName != null)
            {
                return Assembly.Load(migrationsAssemblyName);
            }
            else
            {
                throw new InvalidOperationException("Migrations assembly not found.");
            }
        }

        //private static IServiceCollection AddRepositories(this IServiceCollection services)
        //{
        //    // Add Repositories
        //    services.AddScoped(typeof(IRepository<>), typeof(ApplicationDbRepository<>));

        //    foreach (var aggregateRootType in
        //        typeof(IAggregateRoot).Assembly.GetExportedTypes()
        //            .Where(t => typeof(IAggregateRoot).IsAssignableFrom(t) && t.IsClass)
        //            .ToList())
        //    {
        //        // Add ReadRepositories.
        //        services.AddScoped(typeof(IReadRepository<>).MakeGenericType(aggregateRootType), sp =>
        //            sp.GetRequiredService(typeof(IRepository<>).MakeGenericType(aggregateRootType)));

        //        // Decorate the repositories with EventAddingRepositoryDecorators and expose them as IRepositoryWithEvents.
        //        services.AddScoped(typeof(IRepositoryWithEvents<>).MakeGenericType(aggregateRootType), sp =>
        //            Activator.CreateInstance(
        //                typeof(EventAddingRepositoryDecorator<>).MakeGenericType(aggregateRootType),
        //                sp.GetRequiredService(typeof(IRepository<>).MakeGenericType(aggregateRootType)))
        //            ?? throw new InvalidOperationException($"Couldn't create EventAddingRepositoryDecorator for aggregateRootType {aggregateRootType.Name}"));
        //    }

        //    return services;
        //}
    }
}
