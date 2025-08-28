using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using System.Xml.Linq;


namespace EbayAPI
{
    public class EbayClient
    {

        private readonly EbaySettings settings;

        public EbayClient(EbaySettings settings)
        {

            this.settings = settings;

        }

        public async Task InitializeAsync()
        {
            if (this.settings.Id == null || this.settings.Secret == null) {
                Console.WriteLine("Ebay API client not Set up Properly");
                    return;
            }

            settings.AccessToken = await GetEbayAccessToken(settings.Id, settings.Secret);


        }


        public async Task<string?> GetEbayAccessToken(string clientId, string clientSecret)
        {
            using var http = new HttpClient();

            var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

            var content = new StringContent(
                 "grant_type=client_credentials&scope=https://api.ebay.com/oauth/api_scope",
                 Encoding.UTF8,
                 "application/x-www-form-urlencoded");

            var response = await http.PostAsync("https://api.ebay.com/identity/v1/oauth2/token", content);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("eBay token request for api_scope failed:");
                Console.WriteLine(json);
                return null;
            }

            try
            {
                using var doc = JsonDocument.Parse(json);
                Console.WriteLine("EBay Access Token for api_scope acquired");
                return doc.RootElement.GetProperty("access_token").GetString();
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("eBay token response for api_scope missing 'access_token':");
                Console.WriteLine(json);
                return null;
            }
        }


        public async Task<string?> GetSellerLevelEbayAccessToken()
        {
            using var http = new HttpClient();

            var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{settings.Id}:{settings.Secret}"));
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

            var content = new StringContent(
                 "grant_type=client_credentials&scope=https://api.ebay.com/oauth/api_scope",
                 Encoding.UTF8,
                 "application/x-www-form-urlencoded");

            var response = await http.PostAsync("https://api.ebay.com/identity/v1/oauth2/token", content);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("eBay token request for sell.inventory Scope failed:");
                Console.WriteLine(json);
                return null;
            }

            try
            {
                using var doc = JsonDocument.Parse(json);
                Console.WriteLine("EBay Access Token for sell.inventory Scope acquired");
                return doc.RootElement.GetProperty("access_token").GetString();
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("eBay token response missing for sell.inventory Scope 'access_token':");
                Console.WriteLine(json);
                return null;
            }
        }

        public async Task<Dictionary<string, (string?, decimal?)>> GetEanFromEbayId(List<string> itemIds)
        {
            Dictionary<string, (string?,decimal?)> map = new Dictionary<string, (string? ean, decimal? price)>();

            using var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings.AccessToken);

            int total = itemIds.Count;
            int index = 0;

            foreach (string itemId in itemIds)
            {
                index++;
                string? ean = null;
                string? price = null;

                if (string.IsNullOrEmpty(itemId)) continue;
                var url = $"https://api.ebay.com/buy/browse/v1/item/get_item_by_legacy_id?legacy_item_id={itemId}";
                var response = await http.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error: {response.StatusCode} for itemID {itemId}");
                    continue;
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                // Try to read EAN from gtins
                if (doc.RootElement.TryGetProperty("gtin", out var gtin))
                {
                    ean = gtin.ToString();
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

                if (doc.RootElement.TryGetProperty("price", out var priceElement) && priceElement.TryGetProperty("convertedFromValue", out var convertedValue))
                {
                    price = convertedValue.ToString();
                }

                if(decimal.TryParse(price, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal decimalValue))
                {

                    map.Add(itemId, (ean, decimalValue));
                }
                else map.Add(itemId, (ean, null));

                Console.Write($"\rAdding EANS: {index}/{total} ({(index * 100) / total}%)");
            }
            Console.WriteLine();

            return map;
        }



        public async Task<Dictionary<string, string>> GetAllEbayTitlesAndItemIDsAsync()
        {
            Dictionary<string, string> idTitleMap = new Dictionary<string, string>();
            string xmlData = await GetAllActiveListingsXmlAsync();

            var document = XDocument.Parse(xmlData);

            XNamespace ns = "urn:ebay:apis:eBLBaseComponents";

            var items = document.Descendants(ns + "Item");

            int totalEntries = items.Count();
            int currentElem = 0;
            if (totalEntries == 0) throw new Exception("Number of Active Listings in XML File is 0");

                
            foreach (var item in items)
            {
                currentElem++;
                var itemId = item.Element(ns + "ItemID")?.Value;
                var title = item.Element(ns + "Title")?.Value;

                if (!string.IsNullOrEmpty(itemId) && !string.IsNullOrEmpty(title))
                {
                    idTitleMap[itemId] = title;
                }

                Console.Write($"\rProcessing {currentElem}/{totalEntries} ({(currentElem * 100) / totalEntries}%)");
            }
            return idTitleMap;
        }


        public async Task<string> GetAllActiveListingsXmlAsync()
        {
            const string url = "https://api.ebay.com/ws/api.dll";

            var xmlBody = $@"<?xml version=""1.0"" encoding=""utf-8""?>
                <GetMyeBaySellingRequest xmlns=""urn:ebay:apis:eBLBaseComponents"">
                  <RequesterCredentials>
                    <eBayAuthToken>{settings.SellerToken}</eBayAuthToken>
                  </RequesterCredentials>
                  <ActiveList>
                    <Include>true</Include>
                    <Pagination>
                      <EntriesPerPage>200</EntriesPerPage>
                      <PageNumber>29</PageNumber>
                    </Pagination>
                  </ActiveList>
                </GetMyeBaySellingRequest>";

            using var client = new HttpClient();

            // Set Trading API headers
            client.DefaultRequestHeaders.Add("X-EBAY-API-CALL-NAME", "GetMyeBaySelling");
            client.DefaultRequestHeaders.Add("X-EBAY-API-SITEID", "77"); // Germany (0 = US)
            client.DefaultRequestHeaders.Add("X-EBAY-API-COMPATIBILITY-LEVEL", "967");
            client.DefaultRequestHeaders.Add("X-EBAY-API-DEV-NAME", settings.DevID);
            client.DefaultRequestHeaders.Add("X-EBAY-API-APP-NAME", settings.Id);
            client.DefaultRequestHeaders.Add("X-EBAY-API-CERT-NAME", settings.Secret);
            client.DefaultRequestHeaders.Add("X-EBAY-API-REQUEST-ENCODING", "XML");

            var content = new StringContent(xmlBody, Encoding.UTF8, "text/xml");

            var response = await client.PostAsync(url, content);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(result);
                throw new Exception($"ebay Trading API Call failed: {response.StatusCode}");
            }
            Console.WriteLine("ebay Trading API Call succeeded");
            return result;
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
