namespace Multitenant.Application.Interfaces.Persistance
{
    public interface IDatabaseInitializer
    {
        Task InitializeDatabasesAsync(CancellationToken cancellationToken);
        //Task InitializeApplicationDbForTenantAsync(FSHTenantInfo tenant, CancellationToken cancellationToken);
    }
}
