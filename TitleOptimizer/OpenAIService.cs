using OpenAI;
using OpenAI.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;



namespace TitleOptimizer
{
    public class OpenAiService
    {
        #pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                OpenAIResponseClient client = new(
            model: "gpt-4.1",
            apiKey: "");

        public OpenAiService(){}

        public Dictionary<string, string> GenerateTitles(Dictionary<string, string> ebayDescriptions)
        {
            // 1. Build list with index so we keep track
            var indexed = ebayDescriptions.Select((pair, index) =>
                new {
                    Index = index,
                    EbayId = pair.Key,
                    Description = pair.Value
                }).ToList();

            // 2. Create input for GPT with index|ebayId|description (ID is just for us)
            string productList = string.Join("\n", indexed.Select(x => $"{x.Index + 1}|{x.EbayId}|{x.Description}"));

            string prompt =
                "Erstelle für jeden der folgenden Produkte einen eBay Titel mit max. 80 Zeichen.\n" +
                "Füge keine Nummerierung hinzu, antworte mit genau einem Titel pro Zeile in der Reihenfolge der Eingabe.\n" +
                "Hier sind die Produkte:\n\n" +
                productList;

#pragma warning disable OPENAI001
            OpenAIResponse response = client.CreateResponse(prompt);
#pragma warning restore OPENAI001

            var responseLines = response.GetOutputText()
                .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            if (responseLines.Count != indexed.Count)
                throw new Exception("Mismatch between number of input descriptions and output titles!");

            // 3. Zip back together: ebayId => generated title
            var finalTitles = indexed
                .Select((x, i) => new { x.EbayId, Title = responseLines[i] })
                .ToDictionary(x => x.EbayId, x => x.Title);

            return finalTitles;
        }

    }
}
