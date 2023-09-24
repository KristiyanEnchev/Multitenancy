namespace Multitenant.Application.Identity.User
{
    using Multitenant.Application.Response;

    public class UserListFilter : PaginationFilter
    {
        public bool? IsActive { get; set; }
    }
}