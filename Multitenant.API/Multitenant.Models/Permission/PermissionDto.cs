namespace Multitenant.Models.Permission
{
    public class PermissionDto
    {
        public int? Id { get; set; }
        public string? Description { get; set; }
        public string? Group { get; set; }
        public string? TenantId { get; set; }
        public string? RoleId { get; set; }
        public string? ClaimType { get; set; }
        public string? ClaimValue { get; set; }

        public string? CreatedBy { get; init; }
        public DateTime CreatedOn { get; init; }
    }
}
