namespace Multitenant.Models.HealthCheck
{
    public class Health
    {
        public bool TenantHealthChecks { get; set; } = false;
        public bool DatabaseHealthChecks { get; set; } = false;
        public bool MemoryCacheHealthChecks { get; set; } = false;
    }
}
