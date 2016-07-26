using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stockpile.Api.App;
using Stockpile.Sdk.Models;
using Stockpile.Sdk.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Stockpile.Api.Controllers
{
    public class DataController : BaseController
    {
        public DataController(IOptions<StockpileOptions> stockpileOptions)
            : base(stockpileOptions) { }

        // GET api/data/5
        [HttpGet("{id}")]
        public ActionResult Get(Guid id)
        {
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
                return new NotFoundResult();
            }
            catch (FileNotFoundException e)
            {
                return new NotFoundResult();
            }
            catch (Exception e)
            {
                return new StatusCodeResult(500);
            }
            finally
            {
                //Logging
            }
        }

        // POST api/data
        [HttpPost]
        public async Task<Guid> Post()
        {
            using (var tempStream = GetTempFileStream())
            {
                await Request.Body.CopyToAsync(tempStream);
                
                string storageKey = Retry.Do(() =>
                {
                    tempStream.Position = 0;
                    return StorageAdapter.Create(tempStream);
                }, TimeSpan.FromMilliseconds(200));
                
                Stock stock = new Stock();
                stock.ExternalStorageKey = storageKey;

                var stockKey = GetStockKeyFromHeaders();

                stock = DataProvider.CreateStock(stock, stockKey);
                return stock.Id;
            }
        }

        // PUT api/data/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(Guid id)
        {
            try
            {
                using (var tempStream = GetTempFileStream())
                {
                    var stockKey = GetStockKeyFromHeaders();
                    var stock = DataProvider.RetrieveStock(id, stockKey);
                    if (stock == null)
                        throw new FileNotFoundException("Could not retrieve Stock.", id.ToString());

                    await ControllerContext.HttpContext.Request.Body.CopyToAsync(tempStream);
                    tempStream.Position = 0;

                    if (!StorageAdapter.Update(stock.ExternalStorageKey, tempStream))
                        throw new Exception("Could not update stock.");

                    return new OkResult();
                }
            }
            catch (UnauthorizedAccessException e)
            {
                return new NotFoundResult();
            }
            catch (FileNotFoundException e)
            {
                return new NotFoundResult();
            }
            catch (Exception e)
            {
                return new StatusCodeResult(500);
            }
            finally
            {

            }
            
        }

        // DELETE api/data/5
        [HttpDelete("{id}")]
        public ActionResult Delete(Guid id)
        {
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
                    throw new Exception("Could not delete Stock.");

                return new OkResult();
            }
            catch (UnauthorizedAccessException e)
            {
                return new NotFoundResult();
            }
            catch (FileNotFoundException e)
            {
                return new NotFoundResult();
            }
            catch (Exception e)
            {
                return new StatusCodeResult(500);
            }
            finally
            {

            }
        }
    }
}
