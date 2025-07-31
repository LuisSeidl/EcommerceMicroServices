using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFCore;
using BigBuyAPI;
using Microsoft.EntityFrameworkCore;
using EFCore.Entities;
using System.Diagnostics.Eventing.Reader;


class Program
{
    public static async Task Main(string[] args)
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

        if(batch.Count > 0)
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

