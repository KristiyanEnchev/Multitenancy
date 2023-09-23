namespace Multitenant.Domain.Entities.Identity
{
    using Microsoft.AspNetCore.Identity;

    public class UserRole : IdentityRole
    {
        public string? Description { get; set; }

        public UserRole(string name, string? description = null)
            : base(name)
        {
            Description = description;
            NormalizedName = name.ToUpperInvariant();
        }
    }
}