using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nest;
using Stockpile.Sdk.Interfaces;
using Stockpile.Sdk.Models;

namespace Stockpile.DataProvider.ElasticSearch
{
    public class ElasticSearchDataProvider: IDataProvider
    {
        private readonly ElasticClient _client;
        private const string DEFAULT_INDEX = "stockpile";

        public ElasticSearchDataProvider(string connectionString)
        {
            var settings = new ConnectionSettings(new Uri(connectionString));
            settings.DefaultIndex(DEFAULT_INDEX);
            _client = new ElasticClient(settings);
            Initialize();
        }

        public Stock CreateStock(Stock stock, string stockKey = null)
        {
            if(!string.IsNullOrWhiteSpace(stockKey))
                stock = new LockedStock(stock, stockKey);

            if (stock.Id.Equals(Guid.Empty))
                stock.Id = Guid.NewGuid();

            var result = _client.Index(stock);
            if (result.Created || result.IsValid)
                return stock;
            return null;
        }
        //@@@ Need to resolve issue with LockedStock, combine types require key, throw if present and not supplied.
        public Stock RetrieveStock(Guid id, string stockKey = null)
        {
            var refreshResults = _client.Refresh(DEFAULT_INDEX);
            Stock result = null;
            if (!string.IsNullOrWhiteSpace(stockKey))
            {
                var results = _client.Search<LockedStock>(s => s
                .Size(1)
                .Query(q => q
                    .Term(t => t
                        .Field(f => f.Id)
                        .Value(id))));

                var testResult = results.Documents.FirstOrDefault();

                if (testResult != null && testResult.Key == stockKey)
                    return testResult;
                return null;
            }
            else
            {
                var results = _client.Search<Stock>(s => s
                .Size(1)
                .Query(q => q
                    .Term(t => t
                        .Field(f => f.Id)
                        .Value(id))));

                return results.Documents.FirstOrDefault();
            }
        }

        public bool UpdateStock(Guid id, Stock stock, string stockKey = null)
        {
            var existing = RetrieveStock(id, stockKey);

            if (existing == null)
                return false;

            existing.ExternalStorageKey = stock.ExternalStorageKey;

            var result = _client.Index(existing);
            return result.IsValid && !result.Created;
        }

        public bool DeleteStock(Guid id, string stockKey = null)
        {
            if (!string.IsNullOrWhiteSpace(stockKey))
            {
                var result = _client.Delete(DocumentPath<LockedStock>.Id(id));
                return result.IsValid;
            }
            else
            {
                var result = _client.Delete(DocumentPath<Stock>.Id(id));
                return result.IsValid;
            }   

        }

        protected void Initialize(bool reinitialize = false)
        {
            if (reinitialize)
            {
                var result = _client.DeleteIndex(Indices.All);
                if(!result.Acknowledged)
                    throw new Exception("Unable to initialize database.");
            }

            var indexExists = _client.IndexExists(DEFAULT_INDEX);

            if (!indexExists.Exists)
            {
                var createIndexDescriptor = new CreateIndexDescriptor(DEFAULT_INDEX)
                .Mappings(mappings => mappings
                    .Map<Stock>(m => m
                        .Properties(p => p
                            .String(s => s
                                .Name(n => n.Id)
                                .Analyzer("keyword")
                            )
                        )
                    )
                    .Map<LockedStock>(m => m
                        .Properties(p => p
                            .String(s => s
                                .Name(n => n.Id)
                                .Analyzer("keyword")
                            )
                        )
                    )
                );

                var createIndexResponse = _client.CreateIndex(createIndexDescriptor);

                if(!createIndexResponse.Acknowledged)
                    throw new Exception("Unable to create index.", createIndexResponse.OriginalException);
            }
        }
    }
}
