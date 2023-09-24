namespace Multitenant.Application.Interfaces.Cache
{
    using Multitenant.Application.Interfaces.DependencyScope;

    public interface ICacheKeyService : IScopedService
    {
        public string GetCacheKey(string name, object id, bool includeTenantId = true);
    }
}