namespace Multitenant.Infrastructure.Services.Tenant
{
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.DependencyInjection;

    using Multitenant.Models.Middleware;
    using Multitenant.Application.Interfaces.Tenant;

    public class ExpiryJobRunner : IHostedService, IDisposable
    {
        private Timer? _timer;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ExpiryJobRunner> _logger;
        private readonly IOptionsMonitor<MiddlewareSettings> _backgroundJobSettings;

        public ExpiryJobRunner(IServiceScopeFactory scopeFactory, ILogger<ExpiryJobRunner> logger, IOptionsMonitor<MiddlewareSettings> backgroundJobSettings)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _backgroundJobSettings = backgroundJobSettings;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _backgroundJobSettings.OnChange((settings, _) =>
            {
                if (settings.EnableBackgroundExpiryJobRunner)
                {
                    _timer = new Timer(async (_) => await DoWorkAsync(), null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
                }
                else
                {
                    _timer?.Change(Timeout.Infinite, 0);
                }
            });

            return Task.CompletedTask;
        }

        private async Task DoWorkAsync()
        {
            try
            {
                _logger.LogInformation("ExpiryJobRunner is running at: {time}", DateTimeOffset.Now);

                using (var scope = _scopeFactory.CreateScope())
                {
                    var tenantService = scope.ServiceProvider.GetRequiredService<ITenantService>();
                    var tenants = await tenantService.GetAllAsync();

                    foreach (var tenant in tenants)
                    {
                        if (tenant.ValidUpto <= DateTime.Now && tenant.IsActive)
                        {
                            await tenantService.DeactivateAsync(tenant.Id);

                            _logger.LogInformation($"{tenant.Name}:   Deactivated");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in ExpiryJobRunner: {message}", ex.Message);
                throw new Exception(ex.Message);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
