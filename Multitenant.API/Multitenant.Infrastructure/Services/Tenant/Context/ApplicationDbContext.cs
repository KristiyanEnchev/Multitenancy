namespace Multitenant.Infrastructure.Services.Tenant.Context
{
    using Microsoft.Extensions.Options;
    using Microsoft.EntityFrameworkCore;

    using Finbuckle.MultiTenant;

    using Multitenant.Models.Persistence;
    using Multitenant.Application.Events;
    using Multitenant.Application.Interfaces.Utility.User;
    using Multitenant.Application.Interfaces.Utility.Serializer;

    public class ApplicationDbContext : BaseDbContext
    {
        public ApplicationDbContext(ITenantInfo currentTenant, DbContextOptions options, ICurrentUser currentUser, ISerializerService serializer, IOptions<DatabaseSettings> dbSettings, IEventPublisher events)
            : base(currentTenant, options, currentUser, serializer, dbSettings, events)
        {
        }

        //public DbSet<Product> Products => Set<Product>();
        //public DbSet<Brand> Brands => Set<Brand>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.HasDefaultSchema(SchemaNames.Catalog);
        }
    }
}
