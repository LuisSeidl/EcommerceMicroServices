using EFCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EbayCSVHandler;

namespace EbayCSVHandler
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

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            try
            {
                EbayCsvRow ebayCsvRow = new EbayCsvRow();
                kickflipDBContext dBContext = new kickflipDBContext();
                var newTitles = dBContext.Products
                    .Where(p => !string.IsNullOrWhiteSpace(p.id) && !string.IsNullOrWhiteSpace(p.ebayTitle))
                    .ToDictionary(p => p.id!, p => p.ebayTitle!);

                string input = "C:\\Users\\luiss\\Downloads\\eBay-edit-price-quantity-template-2025-08-05-13241595568.csv";
                string output = "C:\\Users\\luiss\\output.csv";
                ebayCsvRow.UpdateEbayTitlesinCsv(input, output, newTitles);

            }
            catch (Exception ex) {
                _logger.LogWarning($" Exception trying to write csv Data, Message: {ex}");            
            }

            return Task.CompletedTask;
        }
    }
}
