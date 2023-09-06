using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multitenant.WEB.Controllers
{
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

            if (result.Status == HealthStatus.Healthy)
            {
                return Ok(result);
            }

            return Ok(result);

            return StatusCode(500, result);
        }
    }
}
