namespace Multitenant.Application.Identity.Token
{
    public record RefreshTokenRequest(string Token, string RefreshToken);
}