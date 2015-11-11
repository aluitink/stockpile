using Microsoft.AspNet.Mvc.Filters;
using Microsoft.Framework.Logging;

namespace Stockpile.Api.App
{
    public class LastChanceExceptionHandler: ActionFilterAttribute, IExceptionFilter
    {
        private static ILogger _logger;
        public LastChanceExceptionHandler(ILoggerFactory loggerFactory)
        {
            if(_logger == null)
                _logger = loggerFactory.CreateLogger<LastChanceExceptionHandler>();
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogCritical("Uncaught Exception", context.Exception);
        }
    }
}