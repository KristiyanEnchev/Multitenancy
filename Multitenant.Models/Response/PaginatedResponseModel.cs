namespace Multitenant.Models.Response
{
    using System.Collections.Generic;

    public abstract class PaginatedResponseModel<TResponseModel>
    {
        protected internal PaginatedResponseModel(
            IEnumerable<TResponseModel> models,
            int page,
            int totalPages)
        {
            Models = models;
            Page = page;
            TotalPages = totalPages;
        }

        public IEnumerable<TResponseModel> Models { get; }

        public int Page { get; }

        public int TotalPages { get; }
    }
}