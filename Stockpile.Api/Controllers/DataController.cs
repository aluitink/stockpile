using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
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
        public FileStreamResult Get(Guid id)
        {
            var stock = DataProvider.RetrieveStock(id);
            var stream = StorageAdapter.Read(stock.ExternalStorageKey);

            return new FileStreamResult(stream, "application/octet-stream");
        }

        // POST api/data
        [HttpPost]
        public async Task<Guid> Post()
        {
            using (var tempStream = GetTempFileStream())
            {
                await HttpContextService.HttpContext.Request.Body.CopyToAsync(tempStream);

                tempStream.Position = 0;

                var storageKey = StorageAdapter.Create(tempStream);
                Stock stock = new Stock();
                stock.ExternalStorageKey = storageKey;

                stock = DataProvider.CreateStock(stock);

                return stock.Id;
            }
        }

        // PUT api/data/5
        [HttpPut("{id}")]
        public async Task<bool> Put(Guid id)
        {
            using (var tempStream = GetTempFileStream())
            {
                await HttpContextService.HttpContext.Request.Body.CopyToAsync(tempStream);
                tempStream.Position = 0;

                var stock = DataProvider.RetrieveStock(id);

                return StorageAdapter.Update(stock.ExternalStorageKey, tempStream);
            }
        }

        // DELETE api/data/5
        [HttpDelete("{id}")]
        public bool Delete(Guid id)
        {
            var stock = DataProvider.RetrieveStock(id);

            var success = StorageAdapter.Delete(stock.ExternalStorageKey);

            if(success)
                success &= DataProvider.DeleteStock(id);

            return success;
        }
    }
}
