namespace Multitenant.WEB.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.Diagnostics.HealthChecks;

    using Multitenant.Models.HealthCheck;

    public class HealthController : ApiController
    {
        private readonly HealthCheckService _healthCheckService;

        public HealthController(HealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> CheckHealth()
        {
            var result = await _healthCheckService.CheckHealthAsync();
            var healthCheckDto = new HealthResult
            {
                Entries = result.Entries.Select(e => new HealthEntry
                {
                    Name = e.Key,
                    Status = e.Value.Status.ToString(),
                    Duration = e.Value.Duration,
                    Exception = e.Value.Exception?.Message
                }).ToList(),
                Status = result.Status.ToString(),
                TotalChecks = result.Entries.Count
            };

            if (result.Status == HealthStatus.Healthy)
            {
                return Ok(healthCheckDto);
            }

            //var some = new
            //{
            //    Checks = result.Entries.Select(e => new
            //    {
            //        e.Key,
            //    }),
            //    Health = "Healty"
            //};
            //return Ok(some);
            return StatusCode(500, healthCheckDto);
        }
    }
}
