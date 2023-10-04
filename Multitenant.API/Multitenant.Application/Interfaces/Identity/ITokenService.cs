namespace Multitenant.Application.Interfaces.Identity
{
    using Multitenant.Application.Identity.Token;
    using Multitenant.Application.Interfaces.DependencyScope;
    using Multitenant.Domain.Entities.Identity;

    public interface ITokenService : ITransientService
    {
        Task<TokenResponse> GetTokenAsync(TokenRequest request, string ipAddress, CancellationToken cancellationToken);

        Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress);

        Task<TokenResponse> GenerateTokensAndUpdateUser(User user, string ipAddress);
    }
}