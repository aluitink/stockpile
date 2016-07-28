using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Stockpile.Public.Sdk
{
    public class StockpileClient
    {
        private readonly HttpClient _client;
        private const string StockKeyHeader = "X-Stock-Key";

        public StockpileClient(string baseAddress)
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri(string.Format("{0}/api/", baseAddress.TrimEnd('/')));
        }

        protected StockpileClient(string baseAddress, HttpClient client = null)
        {
            _client = client ?? new HttpClient();
            _client.BaseAddress = new Uri(string.Format("{0}/api/", baseAddress.TrimEnd('/')));
        }


        public async Task<Guid> CreateAsync(Stream stream, string stockKey = null)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, "data");
            requestMessage.Content = new StreamContent(stream);

            if (!string.IsNullOrWhiteSpace(stockKey))
                requestMessage.Headers.Add(StockKeyHeader, stockKey);

            var response = await _client.SendAsync(requestMessage);
            if(!response.IsSuccessStatusCode)
                throw new Exception(response.ReasonPhrase);
            var responseString = await response.Content.ReadAsStringAsync();
            responseString = responseString.Replace("\"", "");
            return Guid.Parse(responseString);
        }

        public async Task<Stream> RetrieveAsync(Guid id, string stockKey = null)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, string.Format("data/{0}", id));

            if (!string.IsNullOrWhiteSpace(stockKey))
                requestMessage.Headers.Add(StockKeyHeader, stockKey);
            var response = await _client.SendAsync(requestMessage);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                    throw new FileNotFoundException("Could not find Stock.", id.ToString());

                throw new Exception(response.ReasonPhrase);
            }
            
            return await response.Content.ReadAsStreamAsync();
        }

        public async Task UpdateAsync(Guid id, Stream stream, string stockKey = null)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Put, string.Format("data/{0}", id));
            requestMessage.Content = new StreamContent(stream);

            if (!string.IsNullOrWhiteSpace(stockKey))
                requestMessage.Headers.Add(StockKeyHeader, stockKey);

            var response = await _client.SendAsync(requestMessage);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                    throw new FileNotFoundException("Could not find Stock.", id.ToString());

                throw new Exception(response.ReasonPhrase);
            }
        }

        public async Task DeleteAsync(Guid id, string stockKey = null)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Delete, string.Format("data/{0}", id));

            if (!string.IsNullOrWhiteSpace(stockKey))
                requestMessage.Headers.Add(StockKeyHeader, stockKey);

            var response = await _client.SendAsync(requestMessage);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                    throw new FileNotFoundException("Could not find Stock.", id.ToString());

                throw new Exception(response.ReasonPhrase);
            }
        }
    }
}
