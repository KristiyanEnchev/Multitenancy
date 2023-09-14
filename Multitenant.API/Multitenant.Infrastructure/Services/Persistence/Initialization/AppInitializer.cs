//namespace Multitenant.Infrastructure.Services.Persistence.Initialization
//{
//    using Microsoft.Extensions.Logging;
//    using Microsoft.EntityFrameworkCore;

//    using Finbuckle.MultiTenant;

//    using Multitenant.Infrastructure.Services.Tenant.Context;

//    internal class AppInitializer
//    {
//        private readonly ApplicationDbContext _dbContext;
//        private readonly ITenantInfo _currentTenant;
//        private readonly ApplicationDbSeeder _dbSeeder;
//        private readonly ILogger<AppInitializer> _logger;

//        public AppInitializer(ApplicationDbContext dbContext, ITenantInfo currentTenant, ApplicationDbSeeder dbSeeder, ILogger<AppInitializer> logger)
//        {
//            _dbContext = dbContext;
//            _currentTenant = currentTenant;
//            _dbSeeder = dbSeeder;
//            _logger = logger;
//        }

//        public async Task InitializeAsync(CancellationToken cancellationToken)
//        {
//            if (_dbContext.Database.GetMigrations().Any())
//            {
//                if ((await _dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
//                {
//                    _logger.LogInformation("Applying Migrations for '{tenantId}' tenant.", _currentTenant.Id);
//                    await _dbContext.Database.MigrateAsync(cancellationToken);
//                }

//                if (await _dbContext.Database.CanConnectAsync(cancellationToken))
//                {
//                    _logger.LogInformation("Connection to {tenantId}'s Database Succeeded.", _currentTenant.Id);

//                    await _dbSeeder.SeedDatabaseAsync(_dbContext, cancellationToken);
//                }
//            }
//        }
//    }
//}