using Microsoft.AspNetCore.Http;

namespace Stockpile.Public.Api.App
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