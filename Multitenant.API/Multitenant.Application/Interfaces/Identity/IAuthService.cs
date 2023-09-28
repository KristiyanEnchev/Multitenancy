namespace Multitenant.Application.Interfaces.Identity
{
    using Multitenant.Application.Identity.Token;
    using Multitenant.Application.Identity.UserIdentity;
    using Multitenant.Application.Identity.UserIdentity.Password;
    using Multitenant.Application.Interfaces.DependencyScope;

    public interface IAuthService : ITransientService
    {
        Task<string> RegisterAsync(CreateUserRequest request, string origin);
        Task<TokenResponse> LoginAsync(TokenRequest request, string ipAddress, CancellationToken cancellationToken);
        Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress);
        Task<bool> LogoutAsync(string userEmail);

        Task<string> ConfirmEmailAsync(string userId, string code, string tenant, CancellationToken cancellationToken);
        Task<string> ConfirmPhoneNumberAsync(string userId, string code);

        Task<string> ForgotPasswordAsync(ForgotPasswordRequest request, string origin);
        Task<string> ResetPasswordAsync(ResetPasswordRequest request);
        Task<string> ChangePasswordAsync(ChangePasswordRequest request, string userId);
    }
}