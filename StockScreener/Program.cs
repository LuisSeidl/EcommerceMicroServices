using EFCore;
using Microsoft.EntityFrameworkCore;
using StockScreener;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.Extensions.Configuration;


var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddDbContext<kickflipDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("kickflipDBContext")));

var host = builder.Build();
host.Run();