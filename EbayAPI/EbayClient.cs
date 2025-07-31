using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;


namespace EbayAPI
{
    public class EbayClient
    {
        private string? id;
        private string? secret;
        private string? token;

        public EbayClient(string clientId, string clientSecret)
        {

            id = clientId;
            secret = clientSecret;

        }

        public async Task InitializeAsync()
        {
            if (this.id == null || this.secret == null) {
                Console.WriteLine("Ebay API client not Set up Properly");
                    return;
            }

            token = await GetEbayAccessToken(this.id, this.secret);
        }

        public async Task<string?> GetEbayAccessToken(string clientId, string clientSecret)
        {
            using var http = new HttpClient();

            var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

            var content = new StringContent(
                "grant_type=client_credentials&scope=https://api.ebay.com/oauth/api_scope/buy.browse",
                Encoding.UTF8,
                "application/x-www-form-urlencoded");

            var response = await http.PostAsync("https://api.ebay.com/identity/v1/oauth2/token", content);
            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("access_token").GetString();
        }

        public async Task<Dictionary<string, string?>> GetEanFromEbayId(List<string> itemIds)
        {
            Dictionary<string, string?> map = new Dictionary<string, string?>();

            using var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            foreach (string itemId in itemIds)
            {
                string? ean = null; 

                if (string.IsNullOrEmpty(itemId)) continue;
                var url = $"https://api.ebay.com/buy/browse/v1/item/v1|{itemId}|0";
                var response = await http.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error: {response.StatusCode}");
                    continue;
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                // Try to read EAN from gtins
                if (doc.RootElement.TryGetProperty("gtins", out var gtins) && gtins.GetArrayLength() > 0)
                {
                    ean = gtins[0].ToString();
                }

                // Fallback: Try reading from itemSpecifics
                if (doc.RootElement.TryGetProperty("itemSpecifics", out var specs))
                {
                    foreach (var spec in specs.GetProperty("nameValueList").EnumerateArray())
                    {
                        if (spec.GetProperty("name").GetString().Equals("EAN", StringComparison.OrdinalIgnoreCase))
                        {
                            ean = spec.GetProperty("value")[0].ToString();
                        }
                    }
                }
                map.Add(itemId, ean);
            }

            return map;
        }

        public List<string> ExtractItemId(List<string> urls)
        {
            List<string> result = new List<string>();
            foreach(string url in urls)
            {
                var match = Regex.Match(url, @"(?:/p/|/itm/)(\d+)");
                if (match.Success) result.Add(match.Groups[1].Value);
                else Console.WriteLine($"Failed to Extract Ebay ItemId from {url}");
            }
            return result;

        }
    }
    
}
