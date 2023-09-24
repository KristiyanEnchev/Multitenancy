namespace Multitenant.Application.Identity.UserRequests
{
    using Multitenant.Application.Response;

    public class UserListFilter : PaginationFilter
    {
        public bool? IsActive { get; set; }
    }
}