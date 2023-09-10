namespace Multitenant.Application.Interfaces.Auditing
{
    using Multitenant.Models.Auditing;
    using Multitenant.Application.Interfaces.DependencyScope;

    public interface IAuditService : ITransientService
    {
        Task<List<AuditDto>> GetUserTrailsAsync(Guid userId);
    }
}