using EFCore.Entities;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;



namespace BigBuyAPI
{
    public class BigBuyClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "";
        private readonly string _format;

        public BigBuyClient(string format = "json")
        {
            _httpClient = new HttpClient();
            _format = format;

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

        }

        public async IAsyncEnumerable<BigBuyProduct> StreamAllProductsAsync()
        {
            var url = $"https://api.bigbuy.eu/rest/catalog/products.{_format}";
            var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new Exception($"BigBuy API Error: {response.StatusCode}, {content}");

            }
            
            var stream = await response.Content.ReadAsStreamAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            await foreach(var product in JsonSerializer.DeserializeAsyncEnumerable<BigBuyProduct>(stream, options))
            {
                if(product != null) yield return product;
            }
        }
    }
}

