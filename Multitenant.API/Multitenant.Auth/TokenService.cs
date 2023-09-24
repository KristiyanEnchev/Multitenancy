namespace Multitenant.Auth
{
    using System.Text;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.IdentityModel.Tokens.Jwt;

    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;

    using Multitenant.Models.Security;
    using Multitenant.Shared;
    using Multitenant.Shared.ClaimsPrincipal;
    using Multitenant.Shared.Constants.Multitenancy;
    using Multitenant.Domain.Entities.Identity;
    using Multitenant.Application.Exceptions;
    using Multitenant.Application.Identity.Token;
    using Multitenant.Application.Interfaces.Identity;

    public class TokenService : ITokenService
    {
        private readonly UserManager<Multitenant.Domain.Entities.Identity.User> _userManager;
        private readonly SecuritySettings _securitySettings;
        private readonly JwtSettings _jwtSettings;
        private readonly MultiTenantInfo? _currentTenant;

        public TokenService(
            UserManager<Multitenant.Domain.Entities.Identity.User> userManager,
            IOptions<JwtSettings> jwtSettings,
            MultiTenantInfo? currentTenant,
            IOptions<SecuritySettings> securitySettings)
        {
            _userManager = userManager;
            _jwtSettings = jwtSettings.Value;
            _currentTenant = currentTenant;
            _securitySettings = securitySettings.Value;
        }

        public async Task<TokenResponse> GetTokenAsync(TokenRequest request, string ipAddress, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_currentTenant?.Id)
                || await _userManager.FindByEmailAsync(request.Email.Trim().Normalize()) is not { } user
                || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                throw new UnauthorizedException($"Authentication Failed.");
            }

            if (!user.IsActive)
            {
                throw new UnauthorizedException($"User Not Active. Please contact the administrator.");
            }

            if (_securitySettings.RequireConfirmedAccount && !user.EmailConfirmed)
            {
                throw new UnauthorizedException($"E-Mail not confirmed.");
            }

            if (_currentTenant.Id != MultitenancyConstants.Root.Id)
            {
                if (!_currentTenant.IsActive)
                {
                    throw new UnauthorizedException($"Tenant is not Active. Please contact the Application Administrator.");
                }

                if (DateTime.UtcNow > _currentTenant.ValidUpto)
                {
                    throw new UnauthorizedException($"Tenant Validity Has Expired. Please contact the Application Administrator.");
                }
            }

            return await GenerateTokensAndUpdateUser(user, ipAddress);
        }

        public async Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress)
        {
            var userPrincipal = GetPrincipalFromExpiredToken(request.Token);
            string? userEmail = userPrincipal.GetEmail();
            var user = await _userManager.FindByEmailAsync(userEmail!);
            if (user is null)
            {
                throw new UnauthorizedException($"Authentication Failed.");
            }

            if (user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new UnauthorizedException($"Invalid Refresh Token.");
            }

            return await GenerateTokensAndUpdateUser(user, ipAddress);
        }

        private async Task<TokenResponse> GenerateTokensAndUpdateUser(User user, string ipAddress)
        {
            string token = GenerateJwt(user, ipAddress);

            user.RefreshToken = GenerateRefreshToken();
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays);

            await _userManager.UpdateAsync(user);

            return new TokenResponse(token, user.RefreshToken, user.RefreshTokenExpiryTime);
        }

        private string GenerateJwt(User user, string ipAddress) =>
            GenerateEncryptedToken(GetSigningCredentials(), GetClaims(user, ipAddress));

        private IEnumerable<Claim> GetClaims(User user, string ipAddress) =>
            new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email!),
                new(LocalAppClaims.Fullname, $"{user.FirstName} {user.LastName}"),
                new(ClaimTypes.Name, user.FirstName ?? string.Empty),
                new(ClaimTypes.Surname, user.LastName ?? string.Empty),
                new(LocalAppClaims.IpAddress, ipAddress),
                new(LocalAppClaims.Tenant, _currentTenant!.Id),
                new(LocalAppClaims.ImageUrl, user.ImageUrl ?? string.Empty),
                new(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty)
            };

        private static string GenerateRefreshToken()
        {
            byte[] randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private string GenerateEncryptedToken(SigningCredentials signingCredentials, IEnumerable<Claim> claims)
        {
            var token = new JwtSecurityToken(
               claims: claims,
               expires: DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpirationInMinutes),
               signingCredentials: signingCredentials);
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key)),
                ValidateIssuer = false,
                ValidateAudience = false,
                RoleClaimType = ClaimTypes.Role,
                ClockSkew = TimeSpan.Zero,
                ValidateLifetime = false
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(
                    SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
            {
                throw new UnauthorizedException($"Invalid Token.");
            }

            return principal;
        }

        private SigningCredentials GetSigningCredentials()
        {
            byte[] secret = Encoding.UTF8.GetBytes(_jwtSettings.Key);
            return new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256);
        }
    }
}