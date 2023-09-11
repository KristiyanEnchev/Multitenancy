namespace Multitenant.Infrastructure.Services.Persistence.Configuration
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    using Finbuckle.MultiTenant.EntityFrameworkCore;

    using Multitenant.Models.Persistence;
    using Multitenant.Application.Persistence.Auditing;

    public class AuditTrailConfig : IEntityTypeConfiguration<Trail>
    {
        public void Configure(EntityTypeBuilder<Trail> builder) =>
            builder
                .ToTable("AuditTrails", SchemaNames.Auditing)
                .IsMultiTenant();
    }
}