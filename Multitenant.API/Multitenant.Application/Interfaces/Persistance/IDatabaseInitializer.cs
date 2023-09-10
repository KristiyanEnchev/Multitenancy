namespace Multitenant.Application.Interfaces.Persistance
{
    using Multitenant.Shared;

    public interface IDatabaseInitializer
    {
        Task InitializeDatabasesAsync(CancellationToken cancellationToken);
        Task InitializeApplicationDbForTenantAsync(MultiTenantInfo tenant, CancellationToken cancellationToken);
    }
}