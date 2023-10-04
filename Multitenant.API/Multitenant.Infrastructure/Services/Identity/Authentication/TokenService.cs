namespace Multitenant.Infrastructure.Services.Identity.Authentication
{
    using System.Text;
    using System.Security.Claims;
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
        private readonly UserManager<User> _userManager;
        private readonly SecuritySettings _securitySettings;
        private readonly JwtSettings _jwtSettings;
        private readonly MultiTenantInfo? _currentTenant;

        public TokenService(
            UserManager<User> userManager,
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

        public async Task<TokenResponse> GenerateTokensAndUpdateUser(User user, string ipAddress)
        {
            string token = await GenerateJwt(user, ipAddress);

            var newRefreshToken = await GenerateRefreshToken(user);
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays);

            await _userManager.UpdateAsync(user);

            return new TokenResponse(token, user.RefreshTokenExpiryTime, newRefreshToken);
        }

        public async Task<string> GenerateRefreshToken(User user)
        {
            await _userManager.RemoveAuthenticationTokenAsync(user, "MultiAuth", "RefreshToken");
            string newRefreshToken = await _userManager.GenerateUserTokenAsync(user, "MultiAuth", "RefreshToken");
            IdentityResult result = await _userManager.SetAuthenticationTokenAsync(user, "MultiAuth", "RefreshToken", newRefreshToken);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);

                throw new UnauthorizedException($"{string.Join(Environment.NewLine, errors)}");
            }

            return newRefreshToken;
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

            string oldRefreshToken = await _userManager.GetAuthenticationTokenAsync(user, "MultiAuth", "RefreshToken") ?? "";

            if (oldRefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new UnauthorizedException($"Invalid Refresh Token.");
            }

            return await GenerateTokensAndUpdateUser(user, ipAddress);
        }

        //public async Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress)
        //{
        //    var userPrincipal = GetPrincipalFromExpiredToken(request.Token);
        //    string? userEmail = userPrincipal.GetEmail();
        //    var user = await _userManager.FindByEmailAsync(userEmail!);
        //    if (user is null)
        //    {
        //        throw new UnauthorizedException($"Authentication Failed.");
        //    }

        //    if (user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        //    {
        //        throw new UnauthorizedException($"Invalid Refresh Token.");
        //    }

        //    return await GenerateTokensAndUpdateUser(user, ipAddress);
        //}

        //private async Task<TokenResponse> GenerateTokensAndUpdateUser(User user, string ipAddress)
        //{
        //    string token = GenerateJwt(user, ipAddress);

        //    user.RefreshToken = GenerateRefreshToken();
        //    user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays);

        //    await _userManager.UpdateAsync(user);

        //    return new TokenResponse(token, user.RefreshToken, user.RefreshTokenExpiryTime);
        //}

        private async Task<string> GenerateJwt(User user, string ipAddress) =>
            GenerateEncryptedToken(GetSigningCredentials(), await GetClaims(user, ipAddress));

        private async Task<IEnumerable<Claim>> GetClaims(User user, string ipAddress)
        {
            var claims = new List<Claim>
            {
                new("iss", "https://localhost:7219"),
                new("relying_party", _currentTenant!.Id),
                new("Id", user.Id),
                //new(ClaimTypes.NameIdentifier, user.Id),
                //new(ClaimTypes.Email, user.Email!),
                new("Email", user.Email!),
                new(LocalAppClaims.Fullname, $"{user.FirstName} {user.LastName}"),
                new("FistName", user.FirstName ?? string.Empty),
                //new(ClaimTypes.Name, user.FirstName ?? string.Empty),
                new("LastName", user.LastName ?? string.Empty),
                //new(ClaimTypes.Surname, user.LastName ?? string.Empty),
                new(LocalAppClaims.IpAddress, ipAddress),
                new(LocalAppClaims.Tenant, _currentTenant!.Id),
                new(LocalAppClaims.ImageUrl, user.ImageUrl ?? string.Empty),
                new("MobileNumber", user.PhoneNumber ?? string.Empty),
                //new(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty)
                new("auth_time", DateTimeOffset.UtcNow.ToString("o"))
            };

            var roles = _userManager.GetRolesAsync(user).Result.ToList();
            if (roles.Any())
            {
                var rolesClaimValue = string.Join(",", roles);
                claims.Add(new Claim("Roles", rolesClaimValue));
            }
            //foreach (var role in roles)
            //{
            //    claims.Add(new Claim(ClaimTypes.Role, role));
            //}

            await AddDBClaims(claims, user);

            RemoveClaimIfValueIsempty(claims);

            return claims;
        }

        private void AddClaimIfValueExists(List<Claim> claims, string claimType, string claimValue)
        {
            if (!string.IsNullOrWhiteSpace(claimValue))
            {
                claims.Add(new Claim(claimType, claimValue));
            }
        }

        private async Task AddDBClaims(List<Claim> claims, User user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);

            foreach (var userClaim in userClaims)
            {
                claims.Add(new Claim(userClaim.Type, userClaim.Value));
            }
        }

        private void RemoveClaimIfValueIsempty(List<Claim> claims)
        {
            var claimsToRemove = new List<Claim>();

            foreach (var claim in claims)
            {
                if (string.IsNullOrWhiteSpace(claim.Value))
                {
                    claimsToRemove.Add(claim);
                }
            }

            foreach (var claimToRemove in claimsToRemove)
            {
                claims.Remove(claimToRemove);
            }
        }

        //private static string GenerateRefreshToken()
        //{
        //    byte[] randomNumber = new byte[32];
        //    using var rng = RandomNumberGenerator.Create();
        //    rng.GetBytes(randomNumber);
        //    return Convert.ToBase64String(randomNumber);
        //}

        private string GenerateEncryptedToken(SigningCredentials signingCredentials, IEnumerable<Claim> claims)
        {
            var handler = new JwtSecurityTokenHandler();
            handler.OutboundClaimTypeMap.Clear();
            handler.OutboundClaimTypeMap.Add(ClaimTypes.Email, "Email");

            var token = new JwtSecurityToken(
               issuer: _jwtSettings.Issuer,
               audience: _jwtSettings.Audience,
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
                ValidateIssuer = true,
                ValidateAudience = true,
                RoleClaimType = ClaimTypes.Role,
                ClockSkew = TimeSpan.Zero,
                ValidateLifetime = true,
                ValidAudience = _jwtSettings.Audience,
                ValidIssuer = _jwtSettings.Issuer,
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