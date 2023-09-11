namespace Multitenant.Domain.Events
{
    using Multitenant.Domain.Contracts;
    using Multitenant.Domain.Entities;

    public static class EntityUpdatedEvent
    {
        public static EntityUpdatedEvent<TEntity> WithEntity<TEntity>(TEntity entity)
            where TEntity : IEntity
            => new(entity);
    }

    public class EntityUpdatedEvent<TEntity> : DomainEvent
        where TEntity : IEntity
    {
        internal EntityUpdatedEvent(TEntity entity) => Entity = entity;

        public TEntity Entity { get; }
    }
}