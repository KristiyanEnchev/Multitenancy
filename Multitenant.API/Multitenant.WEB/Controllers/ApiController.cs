//namespace Multitenant.WEB.Controllers
//{
//    using System.Runtime.InteropServices;
//    using System.Threading.Tasks;
//    using Extensions;
//    using MediatR;
//    using Microsoft.AspNetCore.Mvc;
//    using Microsoft.Extensions.DependencyInjection;

//    [ApiController]
//    [Route("[controller]")]
//    public abstract class ApiController : ControllerBase
//    {
//        protected const string Id = "{id}";
//        protected const string PathSeparator = "/";

//        private IMediator? mediator;

//        protected IMediator Mediator
//            => mediator ??= this.HttpContext
//                .RequestServices
//                .GetRequiredService<IMediator>();

//        protected Task<ActionResult<TResult>> Send<TResult>(
//            IRequest<TResult> request)
//            => Mediator.Send(request).ToActionResult();

//        protected Task<ActionResult<TResult>> Send<TResult>(
//            IRequest<Result<TResult>> request)
//            => Mediator.Send(request).ToActionResult();

//        protected Task<ActionResult> Send(
//            IRequest<Result> request)
//            => Mediator.Send(request).ToActionResult();
//    }
//}


namespace Multitenant.WEB.Controllers
{
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;

    [ApiController]
    [Route("api/[controller]")]
    public abstract class ApiController : ControllerBase
    {
        protected const string Id = "{id}";
        protected const string PathSeparator = "/";

        private ISender _mediator = null!;

        protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();
    }
}