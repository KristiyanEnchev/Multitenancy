namespace Multitenant.WEB.Attributes
{
    using Multitenant.Shared.Constants.Multitenancy;

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
