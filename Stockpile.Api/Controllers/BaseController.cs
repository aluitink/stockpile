using System;
using System.IO;
using Microsoft.AspNet.Mvc;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;
using Stockpile.Api.App;
using Stockpile.DataProvider.Redis;
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

        protected IDataProvider DataProvider
        {
            get
            {
                if (_dataProvider == null)
                {
                    Logger.LogDebug(string.Format("DataProviderConnectionString: '{0}'", _stockpileOptions.DataProviderConnectionString));
                    _dataProvider = new RedisDataProvider(_stockpileOptions.DataProviderConnectionString, _stringWriter);
                    Logger.LogDebug(_stringWriter.ToString());
                }
                return _dataProvider;
            }
        }

        protected readonly ILogger Logger;
        private StringWriter _stringWriter = new StringWriter();
        private IStorageAdapter _storageAdapter;
        private static IDataProvider _dataProvider;
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

        /// <summary>
        /// Gets the temporary file stream.
        /// </summary>
        /// <param name="path">Location to store temp file.</param>
        /// <param name="options">The options.</param>
        /// <param name="bufferSize">Size of the buffer.</param>
        /// <returns></returns>
        public static Stream GetTempFileStream(string path = null, FileOptions options = FileOptions.DeleteOnClose, int bufferSize = 8192)
        {
            var tempFile = Path.GetTempFileName();
            return new FileStream(tempFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None, bufferSize, options);
        }
    }
}