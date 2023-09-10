namespace Multitenant.Application.Specification
{
    using Ardalis.Specification;

    using Multitenant.Application.Response;

    public class EntitiesByBaseFilterSpec<T, TResult> : Specification<T, TResult>
    {
        public EntitiesByBaseFilterSpec(BaseFilter filter) =>
            Query.SearchBy(filter);
    }

    public class EntitiesByBaseFilterSpec<T> : Specification<T>
    {
        public EntitiesByBaseFilterSpec(BaseFilter filter) =>
            Query.SearchBy(filter);
    }
}
