namespace Multitenant.WEB.Controllers.Identity
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;

    using Swashbuckle.AspNetCore.Annotations;

    using Multitenant.WEB.Attributes;
    using Multitenant.Application.Identity.Token;
    using Multitenant.Application.Interfaces.Identity;
    using Multitenant.Application.Identity.UserIdentity;

    public class IdentityController : VersionNeutralApiController
    {
        //[HttpPost]
        //[Route(nameof(Login))]
        //[AllowAnonymous]
        //[TenantIdHeader]
        //public async Task<ActionResult<string>> Login()
        //{
        //    throw new CustomException("Alooo logvash li", new List<string> { "logva li ne logva li nz" }, HttpStatusCode.BadRequest);
        //    return Ok("Hello");
        //}

        private readonly IAuthService _authService;

        public IdentityController(IAuthService tokenService) => _authService = tokenService;

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
        //[ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Search))]
        public Task<TokenResponse> RefreshAsync(RefreshTokenRequest request)
        {
            return _authService.RefreshTokenAsync(request, GetIpAddress()!);
        }

        [HttpPost("register")]
        [TenantIdHeader]
        [AllowAnonymous]
        [SwaggerOperation("Anonymous user creates a user.", "")]
        //[ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Register))]
        public Task<string> SelfRegisterAsync(CreateUserRequest request)
        {
            return _authService.CreateAsync(request, GetOriginFromRequest());
        }

        private string GetOriginFromRequest() => $"{Request.Scheme}://{Request.Host.Value}{Request.PathBase.Value}";

        private string? GetIpAddress() =>
            Request.Headers.ContainsKey("X-Forwarded-For")
                ? Request.Headers["X-Forwarded-For"]
                : HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "N/A";
    }
}