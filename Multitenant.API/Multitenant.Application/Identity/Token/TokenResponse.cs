namespace Multitenant.Application.Identity.Token
{
    public record TokenResponse(string Token, DateTime RefreshTokenExpiryTime, string RefreshToken);
}