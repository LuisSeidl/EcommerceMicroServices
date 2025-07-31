using System;
using System.Collections.Generic;
using System.Linq;
using EFCore;
using HtmlAgilityPack;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFCore.Entities;


    public class EbayScraper 
    {
        public static async Task<List<string>?> ScrapeEANAsync(List<string> websites)
        {
            try
            {
                List<string> EANs = new List<string>();

                using var playwright = await Playwright.CreateAsync();
                await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
                var context = await browser.NewContextAsync(new BrowserNewContextOptions
                {
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                "(KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36"
                });

                var page = await context.NewPageAsync();

                
                int pageIndex = 1;
                foreach(string url in websites)
                {
                    Console.WriteLine($"[INFO] - Page {pageIndex}");
                    if(pageIndex%50 == 0) await Task.Delay(30000);
                    await page.GotoAsync(url);

                    var eanElement = page.Locator("dl.ux-labels-values--ean dd span.ux-textspans");
                    if (!await eanElement.IsVisibleAsync())
                    {
                        Console.WriteLine($"There is no EAN for url {url}");
                        continue;
                    }
                    string eanValue = await eanElement.InnerTextAsync();
                    EANs.Add(eanValue);
                    await Task.Delay(1000);
                    pageIndex++;
                }
                return EANs;
            }
            catch (PlaywrightException ex)
            {
                Console.WriteLine(ex);
                return null;

            }
        }  
        
    }

