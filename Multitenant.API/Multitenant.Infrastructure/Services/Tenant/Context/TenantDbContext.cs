namespace Multitenant.Infrastructure.Services.Tenant.Context
{
    using Microsoft.EntityFrameworkCore;

    using Finbuckle.MultiTenant.Stores;

    using Multitenant.Shared;
    using Multitenant.Models.Persistence;

    public class TenantDbContext : EFCoreStoreDbContext<MultiTenantInfo>
    {
        public TenantDbContext(DbContextOptions<TenantDbContext> options)
            : base(options)
        {
            //AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MultiTenantInfo>().ToTable("Tenants", SchemaNames.MultiTenancy);
        }
    }
}