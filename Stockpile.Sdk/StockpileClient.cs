using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Stockpile.Sdk
{
    public class StockpileClient
    {
        private readonly HttpClient _client;

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


        public async Task<Guid> CreateAsync(Stream stream)
        {
            var response = await _client.PostAsync("data", new StreamContent(stream));
            if(!response.IsSuccessStatusCode)
                throw new ApplicationException(response.ReasonPhrase);
            var responseString = await response.Content.ReadAsStringAsync();
            responseString = responseString.Replace("\"", "");
            return Guid.Parse(responseString);
        }

        public async Task<Stream> RetrieveAsync(Guid id)
        {
            return await _client.GetStreamAsync(string.Format("data/{0}", id));
        }

        public async Task UpdateAsync(Guid id, Stream stream)
        {
            var response = await _client.PutAsync(string.Format("data/{0}", id), new StreamContent(stream));
            if (!response.IsSuccessStatusCode)
                throw new ApplicationException(response.ReasonPhrase);
        }

        public async Task DeleteAsync(Guid id)
        {
            var response = await _client.DeleteAsync(string.Format("data/{0}", id));
            if (!response.IsSuccessStatusCode)
                throw new ApplicationException(response.ReasonPhrase);
        }
    }
}
