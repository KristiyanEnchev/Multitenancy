namespace Multitenant.Infrastructure.Services.Identity.Authentication.Util
{
    using Multitenant.Models.Mailing;
    using Multitenant.Domain.Entities.Identity;
    using Multitenant.Application.Interfaces.DependencyScope;

    public interface IUtil : ITransientService
    {
        Task<string> GetEmailVerificationUriAsync(User user, string origin);
        void EnsureValidTenant();
        EmailRequest CreateRegistrationEmailRequest(User user, string emailVerificationUri);
        EmailRequest CreateRessetPasswordEmailRequest(User user, string code, string endpointUri, string passwordResetUrl);
    }
}