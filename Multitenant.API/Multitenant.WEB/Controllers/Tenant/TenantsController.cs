namespace Multitenant.WEB.Controllers.Tenant
{
    using Microsoft.AspNetCore.Mvc;

    using Swashbuckle.AspNetCore.Annotations;

    using Multitenant.Models.Tenant;
    using Multitenant.Shared.ClaimsPrincipal;
    using Multitenant.Application.Multitenancy;
    using Multitenant.WEB.Extensions.Permissions;
    using Microsoft.AspNetCore.Authorization;

    public class TenantsController : VersionNeutralApiController
    {
        [HttpGet]
        [MustHavePermission(Action.View, Resource.Tenants)]
        [SwaggerOperation("Get a list of all tenants.", "")]
        public Task<List<TenantDto>> GetListAsync()
        {
            return Mediator.Send(new GetAllTenantsRequest());
        }

        [HttpGet("{id}")]
        [MustHavePermission(Action.View, Resource.Tenants)]
        [SwaggerOperation("Get tenant details.", "")]
        public Task<TenantDto> GetAsync(string id)
        {
            return Mediator.Send(new GetTenantRequest(id));
        }

        [HttpPost]
        [MustHavePermission(Action.Create, Resource.Tenants)]
        [SwaggerOperation("Create a new tenant.", "")]
        public Task<string> CreateAsync(CreateTenantRequest request)
        {
            return Mediator.Send(request);
        }

        [HttpPut]
        [MustHavePermission(Action.Update, Resource.Tenants)]
        [SwaggerOperation("Update tenant connection string.", "")]
        public Task<string> UpdateConnectionStringAsync(UpdateConnectionStringTenantRequest request)
        {
            return Mediator.Send(request);
        }

        [HttpPost("{id}/activate")]
        [MustHavePermission(Action.Update, Resource.Tenants)]
        [SwaggerOperation("Activate a tenant.", "")]
        //[ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Register))]
        public Task<string> ActivateAsync(string id)
        {
            return Mediator.Send(new ActivateTenantRequest(id));
        }

        [HttpPost("{id}/deactivate")]
        [MustHavePermission(Action.Update, Resource.Tenants)]
        [SwaggerOperation("Deactivate a tenant.", "")]
        //[ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Register))]
        public Task<string> DeactivateAsync(string id)
        {
            return Mediator.Send(new DeactivateTenantRequest(id));
        }

        [HttpPost("{id}/upgrade")]
        [MustHavePermission(Action.UpgradeSubscription, Resource.Tenants)]
        [SwaggerOperation("Upgrade a tenant's subscription.", "")]
        //[ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Register))]
        public async Task<ActionResult<string>> UpgradeSubscriptionAsync(string id, UpgradeSubscriptionRequest request)
        {
            return id != request.TenantId
                ? BadRequest()
                : Ok(await Mediator.Send(request));
        }
    }
}