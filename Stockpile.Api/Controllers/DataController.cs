using System;
using System.IO;
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
        public DataController(HttpContextService httpContextService, IOptions<StockpileOptions> stockpileOptions)
            : base(httpContextService, stockpileOptions) { }

        // GET api/data/5
        [HttpGet("{id}")]
        public ActionResult Get(Guid id)
        {
            Logger.LogDebug(string.Format("Entering Get({0})", id));

            try
            {
                var stockKey = GetStockKeyFromHeaders();
                var stock = DataProvider.RetrieveStock(id, stockKey);
                if (stock == null)
                    throw new FileNotFoundException("Could not retrieve Stock.", id.ToString());

                var stream = StorageAdapter.Read(stock.ExternalStorageKey);
                return new FileStreamResult(stream, "application/octet-stream");
            }
            catch (UnauthorizedAccessException e)
            {
                Logger.LogError("File not found.", e);
                return new HttpNotFoundResult();
            }
            catch (FileNotFoundException e)
            {
                Logger.LogError("File not found.", e);
                return new HttpNotFoundResult();
            }
            catch (Exception e)
            {
                Logger.LogError("Unexpected error.", e);
                return new HttpStatusCodeResult(500);
            }
            finally
            {
                Logger.LogDebug(string.Format("Leaving Get({0})", id));
            }
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

                var stockKey = GetStockKeyFromHeaders();

                stock = DataProvider.CreateStock(stock, stockKey);
                Logger.LogDebug("Leaving Post()");
                return stock.Id;
            }
        }

        // PUT api/data/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(Guid id)
        {
            Logger.LogDebug(string.Format("Entering Put({0})", id));
            try
            {
                using (var tempStream = GetTempFileStream())
                {
                    var stockKey = GetStockKeyFromHeaders();
                    var stock = DataProvider.RetrieveStock(id, stockKey);
                    if (stock == null)
                        throw new FileNotFoundException("Could not retrieve Stock.", id.ToString());

                    await HttpContextService.HttpContext.Request.Body.CopyToAsync(tempStream);
                    tempStream.Position = 0;

                    if (!StorageAdapter.Update(stock.ExternalStorageKey, tempStream))
                        throw new ApplicationException("Could not update stock.");

                    return new HttpOkResult();
                }
            }
            catch (UnauthorizedAccessException e)
            {
                Logger.LogError("File not found.", e);
                return new HttpNotFoundResult();
            }
            catch (FileNotFoundException e)
            {
                Logger.LogError("File not found.", e);
                return new HttpNotFoundResult();
            }
            catch (Exception e)
            {
                Logger.LogError("Unexpected error.", e);
                return new HttpStatusCodeResult(500);
            }
            finally
            {
                Logger.LogDebug(string.Format("Leaving Put({0})", id));
            }
            
        }

        // DELETE api/data/5
        [HttpDelete("{id}")]
        public ActionResult Delete(Guid id)
        {
            Logger.LogDebug(string.Format("Entering Delete({0})", id));

            try
            {
                var stockKey = GetStockKeyFromHeaders();
                var stock = DataProvider.RetrieveStock(id, stockKey);
                if (stock == null)
                    throw new FileNotFoundException("Could not retrieve Stock.", id.ToString());

                var success = StorageAdapter.Delete(stock.ExternalStorageKey);

                if (success)
                    success &= DataProvider.DeleteStock(id, stockKey);

                if (!success)
                    throw new ApplicationException("Could not delete Stock.");

                return new HttpOkResult();
            }
            catch (UnauthorizedAccessException e)
            {
                Logger.LogError("File not found.", e);
                return new HttpNotFoundResult();
            }
            catch (FileNotFoundException e)
            {
                Logger.LogError("File not found.", e);
                return new HttpNotFoundResult();
            }
            catch (Exception e)
            {
                Logger.LogError("Unexpected error.", e);
                return new HttpStatusCodeResult(500);
            }
            finally
            {
                Logger.LogDebug(string.Format("Leaving Delete({0})", id));
            }
        }
    }
}
