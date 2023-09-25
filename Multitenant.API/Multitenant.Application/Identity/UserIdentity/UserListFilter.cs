namespace Multitenant.Application.Identity.UserIdentity
{
    using Multitenant.Application.Response;

    public class UserListFilter : PaginationFilter
    {
        public bool? IsActive { get; set; }
    }
}