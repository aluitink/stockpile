using System;
using System.Linq;
using Stockpile.Sdk.Interfaces;
using Stockpile.Sdk.Models;
using Stockpile.Sdk.Utilities;

namespace Stockpile.DataProvider.Lucandrew
{
    public class LucandrewDataProvider: IDataProvider
    {
        public string RootPath { get; private set; }

        private readonly Database _database;

        public LucandrewDataProvider(string connectionString)
        {
            ParseConnectionString(connectionString);
            _database = InitializeDatabase();
        }

        private Database InitializeDatabase()
        {
            if(string.IsNullOrWhiteSpace(RootPath))
                throw new ApplicationException("Could not determine root path.");

            return new Database(RootPath);
        }

        private void ParseConnectionString(string connectionString)
        {
            string[] parts = connectionString.Split(';');

            foreach (string part in parts)
            {
                string[] pair = part.Split(new[] { '=' }, 2);

                if (pair.Length < 2)
                    continue;

                var key = pair[0];
                var value = pair[1];

                switch (key.ToLowerInvariant())
                {
                    case "data":
                        RootPath = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public Stock CreateStock(Stock stock, string stockKey = null)
        {
            return Retry.Do(() =>
            {
                var stockId = Guid.NewGuid();

                if (!string.IsNullOrWhiteSpace(stockKey))
                {
                    StockKey stockKeyObject = new StockKey();
                    stockKeyObject.Key = stockKey;
                    stockKeyObject.StockId = stockId;
                    var stockKeyObjectReference = _database.Store(stockKeyObject);
                    if(stockKeyObjectReference == null)
                        throw new ApplicationException("Could not store StockKey");
                }

                stock.Id = stockId;
                var objectReference = _database.Store(stock);
                return objectReference.Object;
            }, TimeSpan.FromMilliseconds(100));
        }

        public Stock RetrieveStock(Guid id, string stockKey = null)
        {
            ThrowIfNotAuthorized(id, stockKey);

            return Retry.Do(() =>
            {
                var retVal = _database.Search<Stock>(new { Id = id });

                if (retVal == null)
                    return null;

                var reference = retVal.FirstOrDefault();

                if (reference == null)
                    return null;

                return reference.Object;
            }, TimeSpan.FromMilliseconds(100));
        }

        public bool UpdateStock(Guid id, Stock stock, string stockKey = null)
        {
            ThrowIfNotAuthorized(id, stockKey);

            return Retry.Do(() =>
            {
                var existingReferences = _database.Search<Stock>(new { Id = id });

                if (existingReferences == null)
                    return false;

                var existingReference = existingReferences.FirstOrDefault();

                if (existingReference == null)
                    return false;

                existingReference.Object.ExternalStorageKey = stock.ExternalStorageKey;
                existingReference.Update();

                return true;
            }, TimeSpan.FromMilliseconds(100));
        }

        public bool DeleteStock(Guid id, string stockKey = null)
        {
            ThrowIfNotAuthorized(id, stockKey);

            return Retry.Do(() =>
            {
                var existingReferences = _database.Search<Stock>(new { Id = id });

                if (existingReferences == null)
                    return false;

                var existingReference = existingReferences.FirstOrDefault();

                if (existingReference == null)
                    return false;

                existingReference.Delete();

                return true;
            }, TimeSpan.FromMilliseconds(100));
        }

        private void ThrowIfNotAuthorized(Guid id, string stockKey)
        {
            var authorized = Retry.Do(() =>
            {
                var stockKeyObjectReference = _database.Search<StockKey>(new { StockId = id }).FirstOrDefault();
                if (stockKeyObjectReference == null)
                    return string.IsNullOrWhiteSpace(stockKey);

                return stockKeyObjectReference.Object.Key == stockKey;

            }, TimeSpan.FromMilliseconds(100));

            if (!authorized)
                throw new UnauthorizedAccessException();
        }
    }
}
