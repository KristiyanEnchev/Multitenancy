using Multitenant.Application.Interfaces.DependencyScope;
namespace Multitenant.Application.Interfaces.Mailing
{
    using Multitenant.Models.Mailing;

    public interface IEmailService : IScopedService
    {
        Task<bool> SendRegistrationEmail(EmailRequest req);
        //Task<bool> SendRegistrationEmail(string email, string username, string uri);
    }
}
