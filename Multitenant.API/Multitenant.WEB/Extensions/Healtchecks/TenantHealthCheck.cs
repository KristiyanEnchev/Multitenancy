namespace Multitenant.WEB.Extensions.Healtchecks
{
    using Microsoft.Extensions.Diagnostics.HealthChecks;

    public class TenantHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            // Descoped
            var check = new HealthCheckResult(HealthStatus.Healthy);
            return Task.FromResult(check);
        }
    }
}