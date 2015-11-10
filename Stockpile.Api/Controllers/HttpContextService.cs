using Microsoft.AspNet.Http;

namespace Stockpile.Api.Controllers
{
    public class HttpContextService
    {
        public HttpContext HttpContext { get { return _accessor.HttpContext; } }

        private readonly IHttpContextAccessor _accessor;

        public HttpContextService(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }
    }
}