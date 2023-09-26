namespace Multitenant.WEB.Controllers.Identity
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;

    using Swashbuckle.AspNetCore.Annotations;

    using Multitenant.WEB.Attributes;
    using Multitenant.Application.Identity.Token;
    using Multitenant.Application.Interfaces.Identity;
    using Multitenant.Application.Identity.UserIdentity;
    using Multitenant.Application.Identity.UserIdentity.Password;

    public class IdentityController : VersionNeutralApiController
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public IdentityController(IAuthService tokenService, IUserService userService)
        {
            _authService = tokenService;
            _userService = userService;
        }

        [HttpPost("register")]
        [TenantIdHeader]
        [AllowAnonymous]
        [SwaggerOperation("SelfRegister a user.", "")]
        public Task<string> SelfRegisterAsync(CreateUserRequest request)
        {
            return _authService.CreateAsync(request, GetOriginFromRequest());
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [TenantIdHeader]
        [SwaggerOperation("Request an access token using credentials.", "")]
        public async Task<TokenResponse> GetTokenAsync(TokenRequest request, CancellationToken cancellationToken)
        {
            return await _authService.LoginAsync(request, GetIpAddress()!, cancellationToken);
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        [TenantIdHeader]
        [SwaggerOperation("Request an access token using a refresh token.", "")]
        public Task<TokenResponse> RefreshAsync(RefreshTokenRequest request)
        {
            return _authService.RefreshTokenAsync(request, GetIpAddress()!);
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        [TenantIdHeader]
        [SwaggerOperation("Logs out a user", "")]
        public Task<bool> LogoutAsync(string email)
        {
            return _authService.LogoutAsync(email);
        }

        [HttpGet("confirm-email")]
        [AllowAnonymous]
        [SwaggerOperation("Confirm email address for a user.", "")]
        //[ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Search))]
        public Task<string> ConfirmEmailAsync([FromQuery] string tenant, [FromQuery] string userId, [FromQuery] string code, CancellationToken cancellationToken)
        {
            return _userService.ConfirmEmailAsync(userId, code, tenant, cancellationToken);
        }

        [HttpGet("confirm-phone-number")]
        [AllowAnonymous]
        [SwaggerOperation("Confirm phone number for a user.", "")]
        //[ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Search))]
        public Task<string> ConfirmPhoneNumberAsync([FromQuery] string userId, [FromQuery] string code)
        {
            return _userService.ConfirmPhoneNumberAsync(userId, code);
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [TenantIdHeader]
        [SwaggerOperation("Request a password reset email for a user.", "")]
        //[ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Register))]
        public Task<string> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            return _userService.ForgotPasswordAsync(request, GetOriginFromRequest());
        }

        [HttpPost("reset-password")]
        [SwaggerOperation("Reset a user's password.", "")]
        //[ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Register))]
        public Task<string> ResetPasswordAsync(ResetPasswordRequest request)
        {
            return _userService.ResetPasswordAsync(request);
        }

        private string GetOriginFromRequest() => $"{Request.Scheme}://{Request.Host.Value}{Request.PathBase.Value}";

        private string? GetIpAddress() =>
            Request.Headers.ContainsKey("X-Forwarded-For")
                ? Request.Headers["X-Forwarded-For"]
                : HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "N/A";
    }
}