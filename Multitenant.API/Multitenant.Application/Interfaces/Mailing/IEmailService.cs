namespace Multitenant.Application.Interfaces.Mailing
{
    using Multitenant.Models.Mailing;
    using Multitenant.Application.Interfaces.DependencyScope;

    public interface IEmailService : IScopedService
    {
        Task<HttpResponseMessage> SendRegistrationEmail(EmailRequest req);
        Task<HttpResponseMessage> SendPasswordResetnEmail(EmailRequest req);
    }
}