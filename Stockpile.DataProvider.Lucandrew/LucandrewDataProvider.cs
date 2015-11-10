using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stockpile.Sdk.Interfaces;
using Stockpile.Sdk.Models;

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

        public Stock CreateStock(Stock stock)
        {
            stock.Id = Guid.NewGuid();
            var objectReference = _database.Store(stock);
            return objectReference.Object;
        }

        public Stock RetrieveStock(Guid id)
        {
            var retVal = _database.Search<Stock>(new {Id = id});

            if (retVal == null)
                return null;
            
            var reference = retVal.FirstOrDefault();

            if (reference == null)
                return null;

            return reference.Object;
        }

        public bool UpdateStock(Guid id, Stock stock)
        {
            var existingReferences = _database.Search<Stock>(new {Id = id});

            if (existingReferences == null)
                return false;

            var existingReference = existingReferences.FirstOrDefault();

            if (existingReference == null)
                return false;

            existingReference.Object.ExternalStorageKey = stock.ExternalStorageKey;
            existingReference.Update();

            return true;
        }

        public bool DeleteStock(Guid id)
        {
            var existingReferences = _database.Search<Stock>(new { Id = id });

            if (existingReferences == null)
                return false;

            var existingReference = existingReferences.FirstOrDefault();

            if (existingReference == null)
                return false;

            existingReference.Delete();

            return true;
        }
    }
}
