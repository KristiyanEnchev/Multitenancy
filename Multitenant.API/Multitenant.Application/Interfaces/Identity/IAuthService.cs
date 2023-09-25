namespace Multitenant.Application.Interfaces.Identity
{
    using Multitenant.Application.Identity.Token;
    using Multitenant.Application.Identity.UserIdentity;
    using Multitenant.Application.Interfaces.DependencyScope;

    public interface IAuthService : ITransientService
    {
        Task<TokenResponse> LoginAsync(TokenRequest request, string ipAddress, CancellationToken cancellationToken);
        Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress);
        Task<string> CreateAsync(CreateUserRequest request, string origin);
    }
}
