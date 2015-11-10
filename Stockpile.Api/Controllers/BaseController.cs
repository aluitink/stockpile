using Microsoft.AspNet.Mvc;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;
using Stockpile.Api.App;
using Stockpile.Sdk.Interfaces;

namespace Stockpile.Api.Controllers
{
    [Route("api/[controller]")]
    public class BaseController : Controller
    {
        protected HttpContextService HttpContextService { get; set; }
        protected ILibraryManager LibraryManager { get; private set; }
        protected IStorageAdapter StorageAdapter
        {
            get
            {
                return _storageAdapter ?? (_storageAdapter = StorageAdapterFactory.GetAdapter(LibraryManager, _stockpileOptions.StorageAdapter, _stockpileOptions.StorageAdapterConnectionString));
            }
        }

        protected readonly ILogger Logger;

        private IStorageAdapter _storageAdapter;
        private readonly StockpileOptions _stockpileOptions;
        
        public BaseController(HttpContextService httpContextService, IOptions<StockpileOptions> stockpileOptions)
        {
            if (httpContextService != null)
                HttpContextService = httpContextService;

            if (HttpContextService.HttpContext != null)
            {
                var loggingFactory = (ILoggerFactory)HttpContextService.HttpContext.ApplicationServices.GetService(typeof(ILoggerFactory));
                if (loggingFactory != null)
                    Logger = loggingFactory.CreateLogger(GetType().Name);

                var libraryManager = (ILibraryManager)HttpContextService.HttpContext.ApplicationServices.GetService(typeof(ILibraryManager));
                if (libraryManager != null)
                    LibraryManager = libraryManager;
            }
            
            if (stockpileOptions != null)
                _stockpileOptions = stockpileOptions.Value;
        }
    }
}