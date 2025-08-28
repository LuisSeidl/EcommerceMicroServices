using EbayAPI;
using EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAI;
using System;
using TitleOptimizer;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("C:\\Users\\luiss\\source\\repos\\BigBuyStockScreener\\EbayAPI\\token.json", optional: false, reloadOnChange: true);

builder.Services.Configure<EbaySettings>(builder.Configuration.GetSection("Ebay"));

builder.Services.AddDbContext<kickflipDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("kickflipDBContext")));

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();