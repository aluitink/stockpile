using System;
using CSRedis;
using Stockpile.Sdk.Interfaces;
using Stockpile.Sdk.Models;

namespace Stockpile.DataProvider.Redis
{
    public class RedisDataProvider: IDataProvider, IDisposable
    {

        protected RedisClient Client
        {
            get { return _client ?? (_client = new RedisClient(_connectionString)); }
        }
        
        private static RedisClient _client;
        private readonly string _connectionString;

        public RedisDataProvider(string connectionString)
        {
            _connectionString = connectionString;
        }
        
        public Stock CreateStock(Stock stock)
        {
            stock.Id = Guid.NewGuid();
            Client.HMSet(stock.Id.ToString(), stock);
            return stock;
        }

        public Stock RetrieveStock(Guid id)
        {
            return Client.Exists(id.ToString()) ? Client.HGetAll<Stock>(id.ToString()) : null;
        }

        public bool UpdateStock(Guid id, Stock stock)
        {
            Client.HMSet(id.ToString(), stock);
            return true;
        }

        public bool DeleteStock(Guid id)
        {
            var result = Client.Del(id.ToString());
            return result >= 0;
        }

        public void Dispose()
        {
            if (_client != null)
                _client.Dispose();
        }

    }
}
