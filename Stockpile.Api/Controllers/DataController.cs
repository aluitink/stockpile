using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.OptionsModel;
using Stockpile.Api.App;
using Stockpile.Sdk.Models;
using Stockpile.Sdk.Utilities;
using Microsoft.Extensions.Logging;

namespace Stockpile.Api.Controllers
{
    public class DataController : BaseController
    {
        private static object _sync = new object();
        public DataController(HttpContextService httpContextService, IOptions<StockpileOptions> stockpileOptions)
            : base(httpContextService, stockpileOptions) { }

        // GET api/data/5
        [HttpGet("{id}")]
        public ActionResult Get(Guid id)
        {
            Logger.LogDebug(string.Format("Entering Get({0})", id));

            var stock = DataProvider.RetrieveStock(id);

            if (stock != null)
            {
                try
                {
                    var stream = StorageAdapter.Read(stock.ExternalStorageKey);
                    return new FileStreamResult(stream, "application/octet-stream");
                }
                catch (Exception e)
                {
                    Logger.LogError("File not found.", e);
                    return new HttpNotFoundResult();
                }
                finally
                {
                    Logger.LogDebug(string.Format("Leaving Get({0})", id));
                }
            }

            Logger.LogDebug(string.Format("Leaving Get({0}) - NotFound", id));
            return new HttpNotFoundResult();
        }

        // POST api/data
        [HttpPost]
        public async Task<Guid> Post()
        {
            Logger.LogDebug("Entering Post()");
            using (var tempStream = GetTempFileStream())
            {
                await HttpContextService.HttpContext.Request.Body.CopyToAsync(tempStream);
                
                string storageKey = Retry.Do(() =>
                {
                    tempStream.Position = 0;
                    return StorageAdapter.Create(tempStream);
                }, TimeSpan.FromMilliseconds(200));
                
                Stock stock = new Stock();
                stock.ExternalStorageKey = storageKey;

                stock = DataProvider.CreateStock(stock);
                Logger.LogDebug("Leaving Post()");
                return stock.Id;
            }
        }

        // PUT api/data/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(Guid id)
        {
            Logger.LogDebug(string.Format("Entering Put({0})", id));
            using (var tempStream = GetTempFileStream())
            {
                var stock = DataProvider.RetrieveStock(id);

                if (stock != null)
                {
                    await HttpContextService.HttpContext.Request.Body.CopyToAsync(tempStream);
                    tempStream.Position = 0;

                    if (!StorageAdapter.Update(stock.ExternalStorageKey, tempStream))
                        throw new ApplicationException("Could not update stock.");

                    Logger.LogDebug(string.Format("Leaving Put({0})", id));
                    return new HttpOkResult();
                }

                Logger.LogDebug(string.Format("Leaving Put({0}) - NotFound", id));
                return new HttpNotFoundResult();
            }
        }

        // DELETE api/data/5
        [HttpDelete("{id}")]
        public ActionResult Delete(Guid id)
        {
            Logger.LogDebug(string.Format("Entering Delete({0})", id));
            var stock = DataProvider.RetrieveStock(id);

            if (stock != null)
            {
                var success = StorageAdapter.Delete(stock.ExternalStorageKey);

                if (success)
                    success &= DataProvider.DeleteStock(id);

                if (!success)
                    throw new ApplicationException("Could not delete stock.");
            }
            
            Logger.LogDebug(string.Format("Leaving Delete({0})", id));
            return new HttpOkResult();
        }
    }
}
