namespace Multitenant.Domain.Entities.Identity
{
    using Microsoft.AspNetCore.Identity;

    public class RoleClaim : IdentityRoleClaim<string>
    {
        public string? Description { get; set; }

        public string? CreatedBy { get; init; }
        public DateTime CreatedOn { get; init; }
    }
}