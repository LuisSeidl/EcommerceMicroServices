using EFCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using csvHandling;
using EbayAPI;

namespace StockScreener
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<kickflipDBContext>();
                    CSVData cSVData = new CSVData();

                    List<string>? urls = cSVData.getCSVData("C:\\Users\\luiss\\OneDrive\\Desktop\\Small Code\\KickFlip\\NewUrls.csv");

                    if(urls == null || urls.Count == 0)
                    {
                        Console.WriteLine("[ERROR] - CSV Data is Empty");
                        return;
                    }

                    var ebayAPI = new EbayAPI.EbayClient("ClientID","ClientSecret");
                    await ebayAPI.InitializeAsync();

                    List<string> ebayproductIDs = ebayAPI.ExtractItemId(urls);

                    Dictionary<string, string?> MapIdEan = await ebayAPI.GetEanFromEbayId(ebayproductIDs);
                    List<string?> eans = MapIdEan.Values.ToList();
                    
                    if(eans != null && eans.Count > 0)
                    {
                        cSVData.setCSVData("C:\\Users\\luiss\\OneDrive\\Desktop\\Small Code\\KickFlip\\FiftyFiftyEans.csv", eans);

                    }
                    await dbContext.SaveChangesAsync();

                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during the scraping process.");
            }
        }
    }
}

