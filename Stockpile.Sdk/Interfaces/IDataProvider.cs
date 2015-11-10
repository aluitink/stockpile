using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stockpile.Sdk.Models;

namespace Stockpile.Sdk.Interfaces
{
    public interface IDataProvider
    {
        Stock CreateStock(Stock stock);
        Stock RetrieveStock(Guid id);
        bool UpdateStock(Guid id, Stock stock);
        bool DeleteStock(Guid id);
    }
}
