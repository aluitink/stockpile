namespace Stockpile.Public.Api.App
{
    public class StockpileOptions
    {
        public string StorageAdapter { get; set; }
        public string StorageAdapterConnectionString { get; set; }
        public string DataProviderConnectionString { get; set; }
        public string LoggingDirectory { get; set; }
    }
}