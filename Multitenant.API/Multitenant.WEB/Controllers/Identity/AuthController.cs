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
    using Multitenant.Shared.ClaimsPrincipal;

    public class AuthController : VersionNeutralApiController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService tokenService)
        {
            _authService = tokenService;
        }

        [HttpPost("register")]
        [TenantIdHeader]
        [AllowAnonymous]
        [SwaggerOperation("Register a user.", "")]
        public async Task<string> RegisterAsync(CreateUserRequest request)
        {
            return await _authService.RegisterAsync(request, GetOriginFromRequest());
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [TenantIdHeader]
        [SwaggerOperation("Request an access token using credentials.", "")]
        public async Task<TokenResponse> LoginAsync(TokenRequest request, CancellationToken cancellationToken)
        {
            return await _authService.LoginAsync(request, GetIpAddress()!, cancellationToken);
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        [TenantIdHeader]
        [SwaggerOperation("Request an access token using a refresh token.", "")]
        public async Task<TokenResponse> RefreshAsync(RefreshTokenRequest request)
        {
            return await _authService.RefreshTokenAsync(request, GetIpAddress()!);
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        [TenantIdHeader]
        [SwaggerOperation("Logs out a user", "")]
        public async Task<bool> LogoutAsync(string email)
        {
            return await _authService.LogoutAsync(email);
        }

        [HttpGet("confirm-email")]
        [AllowAnonymous]
        [SwaggerOperation("Confirm email address for a user.", "")]
        //[ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Search))]
        public async Task<string> ConfirmEmailAsync([FromQuery] string tenant, [FromQuery] string userId, [FromQuery] string code, CancellationToken cancellationToken)
        {
            return await _authService.ConfirmEmailAsync(userId, code, tenant, cancellationToken);
        }

        [HttpGet("resend-email")]
        [AllowAnonymous]
        [TenantIdHeader]
        [SwaggerOperation("Confirm email address for a user.", "")]
        public async Task<string> ResendEmailAsync([FromQuery] string email)
        {
            return await _authService.ResendEmailAsync(email, GetOriginFromRequest());
        }

        [HttpGet("confirm-phone-number")]
        [AllowAnonymous]
        [TenantIdHeader]
        [SwaggerOperation("Confirm phone number for a user.", "")]
        //[ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Search))]
        public async Task<string> ConfirmPhoneNumberAsync([FromQuery] string userId, [FromQuery] string code)
        {
            return await _authService.ConfirmPhoneNumberAsync(userId, code);
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [TenantIdHeader]
        [SwaggerOperation("Request a password reset email for a user.", "")]
        //[ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Register))]
        public async Task<string> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            return await _authService.ForgotPasswordAsync(request, GetOriginFromRequest());
        }

        [HttpPost("reset-password")]
        //// need to check why this need to be authenticated nad how it will be authenticated then
        [AllowAnonymous]
        [TenantIdHeader]
        [SwaggerOperation("Reset a user's password.", "")]
        //[ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Register))]
        public async Task<string> ResetPasswordAsync(ResetPasswordRequest request)
        {
            return await _authService.ResetPasswordAsync(request);
        }

        [Authorize]
        [TenantIdHeader]
        [HttpPut("change-password")]
        [SwaggerOperation("Change password of currently logged in user.", "")]
        //[ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Register))]
        public async Task<ActionResult> ChangePasswordAsync(ChangePasswordRequest model)
        {
            if (User.GetUserId() is not { } userId || string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            model.UserId = userId;
            await Mediator.Send(model);
            return Ok();
        }

        private string GetOriginFromRequest()
        {
            string protocol = Request.Headers["X-Forwarded-Proto"]!;
            string prefix = Request.Headers["X-Forwarded-Prefix"]!;

            if (string.IsNullOrEmpty(protocol) || string.IsNullOrEmpty(prefix))
            {
                return $"{Request.Scheme}://{Request.Host.Value}{Request.PathBase.Value}";
            }

            return $"{protocol}://{Request.Host.Value}/{prefix}{Request.PathBase.Value}";
        }
        //private string GetOriginFromRequest() => $"{Request.Scheme}://{Request.Host.Value}{Request.PathBase.Value}";

        private string? GetIpAddress() =>
            Request.Headers.ContainsKey("X-Forwarded-For")
                ? Request.Headers["X-Forwarded-For"]
                : HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "N/A";
    }
}