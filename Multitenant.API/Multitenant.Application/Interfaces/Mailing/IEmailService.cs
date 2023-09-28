using Multitenant.Application.Interfaces.DependencyScope;
namespace Multitenant.Application.Interfaces.Mailing
{
    using Multitenant.Models.Mailing;

    public interface IEmailService : IScopedService
    {
        Task<HttpResponseMessage> SendRegistrationEmail(EmailRequest req);
        Task<HttpResponseMessage> SendPasswordResetnEmail(EmailRequest req);
    }
}
