using BigBuyAPI;
using csvHandling;
using EFCore;
using EFCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdateDatabase
{
    public class UpdateFunctions
    {

        public UpdateFunctions() { }

        public void getCompetitorUrlsFromCSV(string filepath)
        {
            kickflipDBContext context = new kickflipDBContext();
            List<CompetitorProduct> products = context.CompetitorProducts.ToList();

            var productMap = products
                .Where(p => !string.IsNullOrWhiteSpace(p.url))
                .ToDictionary(p => p.url);

            CSVData cSVData = new CSVData();

            List<string>? csvRows = cSVData.getCSVData(filepath);

            if(csvRows == null || csvRows.Count == 0) throw new Exception($"CSV Data in path {filepath} is null or empty");

            int insertedCount = 0;

            foreach (string row in csvRows)
            {
                string cleanedUrl = row.Split('?')[0];
                if (!productMap.ContainsKey(cleanedUrl))
                {
                 
                    products.Add(new CompetitorProduct() { url = cleanedUrl });
                    insertedCount++;
                }
            }

            Console.WriteLine("=== SYNC COMPLETE ===");
            Console.WriteLine($"Inserted: {insertedCount}");

            context.SaveChangesAsync();
            return;
        }

        public async void updateBigBuyStockFromCatalogue()
        {
            var apiClient = new BigBuyClient();
            int batchSize = 1000;
            var batch = new List<BigBuyProduct>(batchSize);

            using var dbContext = new kickflipDBContext();

            var dbProducts = dbContext.bigbuyproducts.ToList();

            var dbMap = dbProducts
                .Where(p => !string.IsNullOrWhiteSpace(p.Sku))
                .ToDictionary(p => p.Sku!);

            var seenSkus = new HashSet<string>();
            int updatedCount = 0;
            int insertedCount = 0;

            await foreach (var apiProduct in apiClient.StreamAllProductsAsync())
            {
                if (string.IsNullOrWhiteSpace(apiProduct.Sku)) continue;

                if (!seenSkus.Add(apiProduct.Sku))
                {
                    Console.WriteLine($"Duplicate SKU from API: {apiProduct.Sku}");
                    continue;
                }

                if (dbMap.TryGetValue(apiProduct.Sku, out var existing))
                {
                    existing.Update(apiProduct);
                    updatedCount++;
                }
                else
                {
                    batch.Add(apiProduct);
                    insertedCount++;
                }


                if (batch.Count >= batchSize)
                {
                    dbContext.bigbuyproducts.AddRange(batch);
                    await dbContext.SaveChangesAsync();
                    dbContext.ChangeTracker.Clear();
                    batch.Clear();
                }
            }

            if (batch.Count > 0)
            {
                dbContext.bigbuyproducts.AddRange(batch);
                await dbContext.SaveChangesAsync();
                dbContext.ChangeTracker.Clear();
            }

            var skusToDelete = dbMap.Keys.Except(seenSkus).ToList();
            var toRemove = dbContext.bigbuyproducts.Where(p => skusToDelete.Contains(p.Sku)).ToList();
            int deletedCount = toRemove.Count;

            if (deletedCount > 0)
            {
                dbContext.bigbuyproducts.RemoveRange(toRemove);
                await dbContext.SaveChangesAsync();
            }

            Console.WriteLine("=== SYNC COMPLETE ===");
            Console.WriteLine($"Updated: {updatedCount}");
            Console.WriteLine($"Inserted: {insertedCount}");
            Console.WriteLine($"Deleted: {deletedCount}");
            Console.WriteLine($"Total API Products Processed: {seenSkus.Count}");
        }
    }
}
