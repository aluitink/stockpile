using System;

namespace Stockpile.Sdk.Models
{
    public class LockedStock: Stock
    {
        public string Key { get; set; }

        public LockedStock() { }

        public LockedStock(Stock stock, string key)
        {
            Id = stock.Id;
            ExternalStorageKey = stock.ExternalStorageKey;
            Key = key;
        }
    }

    public class Stock
    {
        public Guid Id { get; set; }
        public string ExternalStorageKey { get; set; }
    }
}
