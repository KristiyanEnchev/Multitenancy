namespace Multitenant.Application.Specification
{
    using Ardalis.Specification;

    using Multitenant.Domain.Contracts;

    public class AuditableEntitiesByCreatedOnBetweenSpec<T> : Specification<T>
        where T : AuditableEntity
    {
        public AuditableEntitiesByCreatedOnBetweenSpec(DateTime from, DateTime until) =>
            Query.Where(e => e.CreatedOn >= from && e.CreatedOn <= until);
    }
}
