using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;
using Stockpile.Api.App;
using Stockpile.Sdk.Models;

namespace Stockpile.Api.Controllers
{
    public class DataController : BaseController
    {
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
                var stream = StorageAdapter.Read(stock.ExternalStorageKey);

                Logger.LogDebug(string.Format("Leaving Get({0})", id));
                return new FileStreamResult(stream, "application/octet-stream");
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

                tempStream.Position = 0;

                var storageKey = StorageAdapter.Create(tempStream);
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
