namespace Multitenant.Infrastructure.Services.Persistence.Configuration
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    using Finbuckle.MultiTenant.EntityFrameworkCore;

    using Multitenant.Domain.Entities.Identity;
    using Multitenant.Models.Persistence;

    public class ApplicationUserConfig : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder
                .ToTable("Users", SchemaNames.Identity)
                .IsMultiTenant();

            builder
                .Property(u => u.ObjectId)
                    .HasMaxLength(256);
        }
    }

    public class ApplicationRoleConfig : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder) =>
            builder
                .ToTable("Roles", SchemaNames.Identity)
                .IsMultiTenant()
                    .AdjustUniqueIndexes();
    }

    public class ApplicationRoleClaimConfig : IEntityTypeConfiguration<RoleClaim>
    {
        public void Configure(EntityTypeBuilder<RoleClaim> builder) =>
            builder
                .ToTable("RoleClaims", SchemaNames.Identity)
                .IsMultiTenant();
    }

    public class IdentityUserRoleConfig : IEntityTypeConfiguration<IdentityUserRole<string>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder) =>
            builder
                .ToTable("UserRoles", SchemaNames.Identity)
                .IsMultiTenant();
    }

    public class IdentityUserClaimConfig : IEntityTypeConfiguration<IdentityUserClaim<string>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserClaim<string>> builder) =>
            builder
                .ToTable("UserClaims", SchemaNames.Identity)
                .IsMultiTenant();
    }

    public class IdentityUserLoginConfig : IEntityTypeConfiguration<IdentityUserLogin<string>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserLogin<string>> builder) =>
            builder
                .ToTable("UserLogins", SchemaNames.Identity)
                .IsMultiTenant();
    }

    public class IdentityUserTokenConfig : IEntityTypeConfiguration<IdentityUserToken<string>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserToken<string>> builder) =>
            builder
                .ToTable("UserTokens", SchemaNames.Identity)
                .IsMultiTenant();
    }
}