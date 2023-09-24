namespace Multitenant.Application.Interfaces.Identity
{
    using Multitenant.Application.Identity.Token;
    using Multitenant.Application.Interfaces.DependencyScope;

    public interface ITokenService : ITransientService
    {
        Task<TokenResponse> GetTokenAsync(TokenRequest request, string ipAddress, CancellationToken cancellationToken);

        Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress);
    }
}