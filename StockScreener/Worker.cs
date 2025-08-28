using csvHandling;
using EbayAPI;
using EFCore;
using EFCore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace StockScreener
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly EbayClient ebayAPI;

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, IOptions<EbaySettings> ebayOptions)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            ebayAPI = new EbayClient(ebayOptions.Value);
        }

        //we get the ids from our CompetitorProducts and add the EAns and the Price using the Ebay API
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<kickflipDBContext>();

                    List<string?> ebayIds = dbContext.CompetitorProducts
                            .Where(p => p.ebayId != null && p.alreadyRead == false)
                            .Select(p => p.ebayId)
                            .ToList();


                    if (ebayIds == null || ebayIds.Count == 0)
                    {
                        Console.WriteLine("[ERROR] - No EbayID's in Database");
                        return;
                    }
                    else Console.WriteLine($"Urls recieved from the Database: {ebayIds.Count}");


                    await ebayAPI.InitializeAsync();

                    Dictionary<string, (string?,decimal?)> MapIdEan = await ebayAPI.GetEanFromEbayId(ebayIds);

                    int total = MapIdEan.Count;
                    int index = 0;
                    var usedEans = new HashSet<string>();


                    foreach (var kvp in MapIdEan)
                    {
                        var ebayID = kvp.Key;
                        var ean = kvp.Value.Item1;
                        var price = kvp.Value.Item2;

                        var product = dbContext.CompetitorProducts.FirstOrDefault(p => p.ebayId == ebayID);
                        if (product == null)
                            continue;

                        bool modified = false;

                        if (!string.IsNullOrWhiteSpace(ean))
                        {
                            if (usedEans.Contains(ean))
                            {
                                Console.WriteLine($"[DUPLICATE] Skipping EAN {ean} (already used)");
                            }
                            else
                            {
                                product.ean13 = ean;
                                usedEans.Add(ean);
                                modified = true;
                            }
                        }

                        if (price != null && product.sellerPrice != price)
                        {
                            product.sellerPrice = price;
                            modified = true;
                        }

                        if (modified) dbContext.Entry(product).State = EntityState.Modified;

                        Console.Write($"\rProcessing {index}/{total} ({(index * 100) / total}%)");

                    }
                    Console.WriteLine();

                    var pendingChanges = dbContext.ChangeTracker.Entries()
                    .Where(e => e.State == EntityState.Modified || e.State == EntityState.Added).ToList().Count;

                    Console.WriteLine($"Added all EANs, Changes:{pendingChanges}");

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

