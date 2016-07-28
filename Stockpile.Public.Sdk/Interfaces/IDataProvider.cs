using System;
using Stockpile.Public.Sdk.Models;

namespace Stockpile.Public.Sdk.Interfaces
{
    public interface IDataProvider
    {
        Stock CreateStock(Stock stock, string stockKey = null);
        Stock RetrieveStock(Guid id, string stockKey = null);
        bool UpdateStock(Guid id, Stock stock, string stockKey = null);
        bool DeleteStock(Guid id, string stockKey = null);
    }
}
