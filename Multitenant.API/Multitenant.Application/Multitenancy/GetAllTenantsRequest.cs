namespace Multitenant.Application.Multitenancy
{
    using MediatR;

    using Multitenant.Models.Tenant;
    using Multitenant.Application.Interfaces.Tenant;

    public class GetAllTenantsRequest : IRequest<List<TenantDto>>
    {
    }

    public class GetAllTenantsRequestHandler : IRequestHandler<GetAllTenantsRequest, List<TenantDto>>
    {
        private readonly ITenantService _tenantService;

        public GetAllTenantsRequestHandler(ITenantService tenantService) => _tenantService = tenantService;

        public Task<List<TenantDto>> Handle(GetAllTenantsRequest request, CancellationToken cancellationToken) =>
            _tenantService.GetAllAsync();
    }
}