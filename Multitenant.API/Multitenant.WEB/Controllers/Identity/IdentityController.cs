namespace Multitenant.WEB.Controllers.Identity
{
    using System.Net;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;

    using Multitenant.WEB.Attributes;
    using Multitenant.Application.Exceptions;

    public class IdentityController : ApiController
    {
        [HttpPost]
        [Route(nameof(Login))]
        [AllowAnonymous]
        [TenantIdHeader]
        public async Task<ActionResult<string>> Login()
        {
            throw new CustomException("Alooo logvash li", new List<string> { "logva li ne logva li nz" }, HttpStatusCode.BadRequest);
            return Ok("Hello");
        }
    }
}