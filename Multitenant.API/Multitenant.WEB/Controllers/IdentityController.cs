using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multitenant.Application.Exceptions;
using Multitenant.WEB.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Multitenant.WEB.Controllers
{
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