using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using csvHandling;

var builder = Host.CreateApplicationBuilder(args);


builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();