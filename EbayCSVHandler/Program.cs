using EFCore;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EbayCSVHandler;

var builder = Host.CreateApplicationBuilder(args);


builder.Services.AddDbContext<kickflipDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("kickflipDBContext")));

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();





