using Newtonsoft.Json;
using OBT.Core.Abstractions;
using System.Net.Http;
using System.Threading.Tasks;

namespace OBT.Core.Services
{
    public class HttpService : IHttpService
    {
        private readonly IHttpClientFactory clientFactory;

        public HttpService(IHttpClientFactory clientFactory)
        {
            this.clientFactory = clientFactory;
        }

        public async Task<T> GetAsync<T>(string url)
        {
            return await this.SendAsync<T>(HttpMethod.Get, url);
        }

        public async Task<T> PostAsync<T>(string url, object body)
        {
            return await this.SendAsync<T>(
                HttpMethod.Post, url, JsonConvert.SerializeObject(body));
        }

        private async Task<T> SendAsync<T>(HttpMethod httpMethod, string url, string body = null)
        {
            var client = this.clientFactory.CreateClient();
            var request = new HttpRequestMessage(httpMethod, url);

            if (!string.IsNullOrWhiteSpace(body))
            {
                request.Content = new StringContent(body);
            }

            var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(result);
        }
    }
}
