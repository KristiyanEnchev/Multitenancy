namespace Multitenant.Application.Exceptions
{
    using System.Net;

    public class NotFoundException : CustomException
    {
        public NotFoundException(string message)
            : base(message, null, HttpStatusCode.NotFound)
        {
        }
    }
}