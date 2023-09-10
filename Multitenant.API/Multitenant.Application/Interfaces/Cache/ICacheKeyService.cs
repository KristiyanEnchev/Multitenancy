namespace Multitenant.Application.Interfaces.Cache
{
    public interface ICacheKeyService
    {
        public string GetCacheKey(string name, object id, bool includeTenantId = true);
    }
}
