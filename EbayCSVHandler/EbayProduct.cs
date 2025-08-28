using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbayCSVHandler
{
    public class EbayCsvRow
    {
        [Name("Action")]
        public string? Action { get; set; }

        [Name("Category name")]
        public string? CategoryName { get; set; }

        [Name("Item number")]
        public string ItemNumber { get; set; }

        [Name("Title")]
        public string? Title { get; set; }

        [Name("Listing site")]
        public string? ListingSite { get; set; }

        [Name("Currency")]
        public string? Currency { get; set; }

        [Name("Start price")]
        public string? StartPrice { get; set; }

        [Name("Buy It Now price")]
        public string? BuyItNowPrice { get; set; }

        [Name("Available quantity")]
        public string? AvailableQuantity { get; set; }

        [Name("Relationship")]
        public string? Relationship { get; set; }

        [Name("Relationship details")]
        public string? RelationshipDetails { get; set; }

        [Name("Custom label (SKU)")]
        public string? CustomLabel { get; set; } // SKU


        public void UpdateEbayTitlesinCsv(string inputPath, string outputPath, Dictionary<string,string> newTitles)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                IgnoreBlankLines = true,
                Comment = '#',
                AllowComments = true,
                MissingFieldFound = null,
                HeaderValidated = null
            };

            using var reader = new StreamReader(inputPath);
            using var csvReader = new CsvReader(reader, config);
            var records = csvReader.GetRecords<EbayCsvRow>().ToList();

            int totalrows = records.Count;
            int current = 0;

            foreach(var row in records) {
                current++;
                if (newTitles.TryGetValue(row.ItemNumber, out var newTitle))
                {
                    row.Title = newTitle;
                    row.Action = "Revise"; // Ensure correct action
                }

                Console.Write($" Row {current} of {totalrows} | ({(current * 100) / totalrows}%)");
            }
            Console.WriteLine();


            using var writer = new StreamWriter(outputPath);
            using var csvWriter = new CsvWriter(writer, config);
            csvWriter.WriteHeader<EbayCsvRow>();
            csvWriter.NextRecord();
            foreach (var row in records)
            {
                csvWriter.WriteRecord(row);
                csvWriter.NextRecord();
            }

        }
    }
}
