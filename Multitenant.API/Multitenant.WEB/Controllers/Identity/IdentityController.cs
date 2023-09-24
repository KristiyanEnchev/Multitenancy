namespace Multitenant.WEB.Controllers.Identity
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;

    using Swashbuckle.AspNetCore.Annotations;

    using Multitenant.WEB.Attributes;
    using Multitenant.Application.Identity.Token;
    using Multitenant.Application.Interfaces.Identity;

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

        private readonly ITokenService _tokenService;

        public IdentityController(ITokenService tokenService) => _tokenService = tokenService;

        [HttpPost("login")]
        [AllowAnonymous]
        [TenantIdHeader]
        [SwaggerOperation("Request an access token using credentials.", "")]
        public Task<TokenResponse> GetTokenAsync(TokenRequest request, CancellationToken cancellationToken)
        {
            return _tokenService.GetTokenAsync(request, GetIpAddress()!, cancellationToken);
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        [TenantIdHeader]
        [SwaggerOperation("Request an access token using a refresh token.", "")]
        //[ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Search))]
        public Task<TokenResponse> RefreshAsync(RefreshTokenRequest request)
        {
            return _tokenService.RefreshTokenAsync(request, GetIpAddress()!);
        }

        private string? GetIpAddress() =>
            Request.Headers.ContainsKey("X-Forwarded-For")
                ? Request.Headers["X-Forwarded-For"]
                : HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "N/A";
    }
}