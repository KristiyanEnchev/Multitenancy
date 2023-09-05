namespace Multitenant.WEB.Filters
{
    using Multitenant.Shared.Constants.Multitenancy;
    using Multitenant.WEB.Extensions.Swagger;

    public class TenantIdHeaderAttribute : SwaggerHeaderAttribute
    {
        public TenantIdHeaderAttribute()
            : base(
                MultitenancyConstants.TenantIdName,
                "Input your tenant Id to access this API",
                string.Empty,
                true)
        {
        }
    }

}
