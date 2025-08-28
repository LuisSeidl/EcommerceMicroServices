using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csvHandling
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

        //we get the ids from our CompetitorProducts and add the EAns and the Price using the Ebay API
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    CSVData data = new CSVData();

                    var input = data.getCSVData("C:\\Users\\luiss\\Downloads\\eBay-edit-price-quantity-template-2025-08-10-13242751223.csv");
                    if(input == null || input.Count == 0) return;
                    
                    var output = new List<string>();
                    foreach(string sku in input)
                    {
                        string newSKU = sku.Split(":")[0];
                        output.Add(newSKU);
                    }
                    data.setCSVData("C:\\Users\\luiss\\output.csv", data: output);

                    Console.WriteLine($"Finished changing SKUs");    
                    return;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception trying to change the SKU, Message: {ex}");
            }

        }
    }
}
