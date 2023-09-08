namespace Multitenant.Models.Security
{
    public class SecuritySettings
    {
        public string? Provider { get; set; }
        public bool RequireConfirmedAccount { get; set; }
    }
}