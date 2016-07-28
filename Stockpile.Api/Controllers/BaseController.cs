using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stockpile.Api.App;
using Stockpile.DataProvider.ElasticSearch;
using Stockpile.Sdk.Interfaces;

namespace Stockpile.Api.Controllers
{
    [Route("api/[controller]")]
    public class BaseController : Controller
    {
        protected IStorageAdapter StorageAdapter
        {
            get
            {
                Logger.LogInformation("StorageAdapter", _stockpileOptions.StorageAdapter);
                Logger.LogInformation("StorageAdapterConnectionString", _stockpileOptions.StorageAdapterConnectionString);
                return _storageAdapter ?? (_storageAdapter = StorageAdapterFactory.GetAdapter(_stockpileOptions.StorageAdapter, _stockpileOptions.StorageAdapterConnectionString));
            }
        }

        protected IDataProvider DataProvider
        {
            get
            {
                lock (Sync)
                {
                    if (_dataProvider == null)
                    {
                        Logger.LogInformation("DataProviderConnectionString", _stockpileOptions.DataProviderConnectionString);
                        _dataProvider = new ElasticSearchDataProvider(_stockpileOptions.DataProviderConnectionString);
                    }
                }
                return _dataProvider;
            }
        }

        protected ILogger Logger;

        private IStorageAdapter _storageAdapter;
        private static IDataProvider _dataProvider;
        private readonly StockpileOptions _stockpileOptions;

        private const string StockKeyHeader = "X-Stock-Key";
        private static readonly object Sync = new object();

        public BaseController(IOptions<StockpileOptions> stockpileOptions, ILogger logger)
        {
            if (stockpileOptions != null)
                _stockpileOptions = stockpileOptions.Value;
            Logger = logger;
        }

        public string GetStockKeyFromHeaders()
        {
            if (ControllerContext == null)
                return null;

            if (ControllerContext.HttpContext.Request.Headers.ContainsKey(StockKeyHeader))
            {
                return ControllerContext.HttpContext.Request.Headers[StockKeyHeader];
            }

            return null;
        }

        /// <summary>
        /// Gets the temporary file stream.
        /// </summary>
        /// <param name="path">Location to store temp file.</param>
        /// <param name="options">The options.</param>
        /// <param name="bufferSize">Size of the buffer.</param>
        /// <returns></returns>
        public Stream GetTempFileStream(string path = null, FileOptions options = FileOptions.DeleteOnClose, int bufferSize = 8192)
        {
            var tempFile = Path.GetTempFileName();
            return new FileStream(tempFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None, bufferSize, options);
        }
    }
}