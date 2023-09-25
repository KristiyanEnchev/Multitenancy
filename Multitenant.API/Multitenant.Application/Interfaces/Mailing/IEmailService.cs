using Multitenant.Application.Interfaces.DependencyScope;

namespace Multitenant.Application.Interfaces.Mailing
{
    public interface IEmailService : IScopedService
    {
        Task<bool> SendRegistrationEmail(string email, string username, string uri);
    }
}
