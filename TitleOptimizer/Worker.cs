using EbayAPI;
using EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitleOptimizer;

namespace TitleOptimizer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly EbayClient ebay;

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, IOptions<EbaySettings> ebayOptions)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            ebay = new EbayClient(ebayOptions.Value);
        }

        //we get the ids from our CompetitorProducts and add the EAns and the Price using the Ebay API
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var openAi = new OpenAiService();
                    

                    var dbContext = scope.ServiceProvider.GetRequiredService<kickflipDBContext>();

                    var existingProducts = dbContext.Products
                        .Where(p => !string.IsNullOrWhiteSpace(p.id))
                        .ToDictionary(p => p.id!);

                    
                    int total = existingProducts.Count();

                    int updated = 0;

                    const int batchSize = 20;

                    for(int i = 0; i < total; i += batchSize)
                    {
                        var batch = existingProducts.Skip(i).Take(batchSize).ToList();

                        var inputDict = batch.ToDictionary(p => p.Value.id, p => p.Value.ebayTitle);

                        Dictionary<string, string> newTitles;

                        try
                        {
                            newTitles = openAi.GenerateTitles(inputDict);
                        }
                        catch (Exception ex) 
                        {
                            Console.WriteLine($"❌ Failed to generate titles for batch {i / batchSize + 1}: {ex.Message}");
                            continue;
                        }

                        foreach (var product in batch) 
                        {
                            var prod = product.Value;
                            if (newTitles.TryGetValue(prod.id, out string newTitle) && prod.ebayTitle != newTitle)
                            {
                                if(newTitle.Length > 80)
                                {
                                    Console.WriteLine($" For product {prod.id} the new Title is too long {newTitle}");
                                    continue;
                                }
                                prod.ebayTitle = newTitle;
                                prod.lastUpdated = DateTime.UtcNow;
                                dbContext.Entry(prod).State = EntityState.Modified;
                                updated++;
                            }
                        }
                        await dbContext.SaveChangesAsync();
                        Console.WriteLine($"Batch {(i / batchSize) + 1}: Updated {updated} titles.");
                    }

                    Console.WriteLine($"Finished improving Titles, total updates: {updated}");    
                    return;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception trying to optimize the Title, Message: {ex}");
            }

        }
    }
}
