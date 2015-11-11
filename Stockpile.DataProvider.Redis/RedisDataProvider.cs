using System;
using MsgPack;
using StackExchange.Redis;
using Stockpile.Sdk.Interfaces;
using Stockpile.Sdk.Models;

namespace Stockpile.DataProvider.Redis
{
    public class RedisDataProvider: IDataProvider, IDisposable
    {
        protected ConnectionMultiplexer Muxer
        {
            get
            {
                return _connectionMultiplexer ??
                       (_connectionMultiplexer = ConnectionMultiplexer.Connect(_connectionString));
            }
        }

        protected IDatabase Database
        {
            get { return Muxer.GetDatabase(); }
        }

        protected ObjectPacker Packer
        {
            get { return _packer ?? (_packer = new ObjectPacker()); }
        }

        private static ConnectionMultiplexer _connectionMultiplexer;
        private static ObjectPacker _packer;
        private readonly string _connectionString;

        public RedisDataProvider(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Dispose()
        {
            if(_connectionMultiplexer != null)
                _connectionMultiplexer.Dispose();
        }

        public Stock CreateStock(Stock stock)
        {
            var id = Guid.NewGuid();
            stock.Id = id;
            var data = Packer.Pack(stock);

            if(!Database.StringSet(id.ToByteArray(), data))
                throw new ApplicationException("Could not store value.");

            return stock;
        }

        public Stock RetrieveStock(Guid id)
        {
            byte[] data = Database.StringGet(id.ToByteArray());
            if (data == null)
                return null;
            Stock stock = Packer.Unpack<Stock>(data);
            return stock;
        }

        public bool UpdateStock(Guid id, Stock stock)
        {
            var data = Packer.Pack(stock);
            return Database.StringSet(id.ToByteArray(), data);
        }

        public bool DeleteStock(Guid id)
        {
            return UpdateStock(id, null);
        }
    }
}
