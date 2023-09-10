namespace Multitenant.Infrastructure.Services.Tenant
{
    using Microsoft.Extensions.Options;

    using Finbuckle.MultiTenant;

    using Mapster;

    using Multitenant.Application.Interfaces.Tenant;
    using Multitenant.Models.Tenant;
    using Multitenant.Application.Exceptions;
    using Multitenant.Application.Interfaces.Persistance;
    using Multitenant.Shared;
    using Multitenant.Models.Persistence;
    using Multitenant.Application.Multitenancy;

    public class TenantService : ITenantService
    {
        private readonly IMultiTenantStore<MultiTenantInfo> _tenantStore;
        private readonly IConnectionStringSecurer _csSecurer;
        private readonly IDatabaseInitializer _dbInitializer;
        private readonly DatabaseSettings _dbSettings;

        public TenantService(IMultiTenantStore<MultiTenantInfo> tenantStore, IConnectionStringSecurer csSecurer, IDatabaseInitializer dbInitializer, IOptions<DatabaseSettings> dbSettings)
        {
            _tenantStore = tenantStore;
            _csSecurer = csSecurer;
            _dbInitializer = dbInitializer;
            _dbSettings = dbSettings.Value;
        }

        public async Task<List<TenantDto>> GetAllAsync()
        {
            var tenants = (await _tenantStore.GetAllAsync()).Adapt<List<TenantDto>>();

            tenants.ForEach(t => t.ConnectionString = _csSecurer.MakeSecure(t.ConnectionString));

            return tenants;
        }

        public async Task<bool> ExistsWithIdAsync(string id)
        {
            return await _tenantStore.TryGetAsync(id) is not null;
        }

        public async Task<bool> ExistsWithNameAsync(string name)
        {
            return (await _tenantStore.GetAllAsync()).Any(t => t.Name == name);
        }

        public async Task<TenantDto> GetByIdAsync(string id)
        {
            return (await GetTenantInfoAsync(id)).Adapt<TenantDto>();
        }

        public async Task<string> CreateAsync(CreateTenantRequest request, CancellationToken cancellationToken)
        {
            if (request.ConnectionString?.Trim() == _dbSettings.ConnectionString.Trim()) request.ConnectionString = string.Empty;

            var tenant = new MultiTenantInfo(request.Id, request.Name, request.ConnectionString, request.AdminEmail, request.Issuer);

            await _tenantStore.TryAddAsync(tenant);

            try
            {
                await _dbInitializer.InitializeApplicationDbForTenantAsync(tenant, cancellationToken);
            }
            catch
            {
                await _tenantStore.TryRemoveAsync(request.Id);

                throw;
            }

            return tenant.Id;
        }

        public async Task<string> ActivateAsync(string id)
        {
            var tenant = await GetTenantInfoAsync(id);

            if (tenant.IsActive)
            {
                throw new ConflictException("Tenant is already Activated.");
            }

            tenant.Activate();

            await _tenantStore.TryUpdateAsync(tenant);

            return $"Tenant {id} is now Activated.";
        }

        public async Task<string> DeactivateAsync(string id)
        {
            var tenant = await GetTenantInfoAsync(id);

            if (!tenant.IsActive)
            {
                throw new ConflictException("Tenant is already Deactivated.");
            }

            tenant.Deactivate();

            await _tenantStore.TryUpdateAsync(tenant);

            return $"Tenant {id} is now Deactivated.";
        }

        public async Task<string> UpdateSubscription(string id, DateTime extendedExpiryDate)
        {
            var tenant = await GetTenantInfoAsync(id);

            tenant.SetValidity(extendedExpiryDate);

            await _tenantStore.TryUpdateAsync(tenant);

            return $"Tenant {id}'s Subscription Upgraded. Now Valid till {tenant.ValidUpto}.";
        }

        private async Task<MultiTenantInfo> GetTenantInfoAsync(string id)
        {
            return await _tenantStore.TryGetAsync(id)
                ?? throw new NotFoundException($"{typeof(MultiTenantInfo).Name} {id} Not Found.");
        }
    }
}
