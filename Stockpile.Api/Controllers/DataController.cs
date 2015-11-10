using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Framework.OptionsModel;
using Stockpile.Api.App;

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
            throw new NotImplementedException();
        }

        // POST api/data
        [HttpPost]
        public Guid Post()
        {
            throw new NotImplementedException();
        }

        // PUT api/data/5
        [HttpPut("{id}")]
        public bool Put(Guid id)
        {
            throw new NotImplementedException();
        }

        // DELETE api/data/5
        [HttpDelete("{id}")]
        public bool Delete(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
