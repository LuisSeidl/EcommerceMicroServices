using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using EFCore.Entities;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging.File;
using Serilog;


namespace EFCore;

public partial class kickflipDBContext : DbContext
{
    public kickflipDBContext()
    {
    }


    public kickflipDBContext(DbContextOptions<kickflipDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Product> Products { get; set; }
    public virtual DbSet<BigBuyProduct> bigbuyproducts { get; set; }



    public static readonly ILoggerFactory FileLoggerFactory
    = LoggerFactory.Create(builder =>
    {
        builder
                .AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information)
                .AddFilter("Microsoft.EntityFrameworkCore.ChangeTracking", LogLevel.Debug)
                .AddFile("Logs/efcore-log.txt");
    });


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseLoggerFactory(FileLoggerFactory)
            .UseNpgsql("")
            .EnableSensitiveDataLogging();
    }





    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products", "kickflip");
            entity.Property(e => e.sku).HasColumnName("sku");
            entity.Property(e => e.ean13).HasColumnName("ean13");
            entity.Property(e => e.id).HasColumnName("id");
            entity.Property(e => e.name).HasColumnName("name");
            entity.Property(e => e.manufacturer).HasColumnName("manufacturer");
            entity.Property(e => e.category).HasColumnName("category");
            entity.Property(e => e.condition).HasColumnName("condition");
            entity.Property(e => e.active).HasColumnName("active");
            entity.Property(e => e.ebayTitle).HasColumnName("ebayTitle");
            entity.Property(e => e.wholesalePrice).HasColumnName("wholesalePrice");
            entity.Property(e => e.recommendedPrice).HasColumnName("recommendedPrice");
            entity.Property(e => e.retailPrice).HasColumnName("retailPrice");
            entity.Property(e => e.currentPrice).HasColumnName("currentPrice");
            entity.Property(e => e.competitorPrice).HasColumnName("competitorPrice");

        });


        modelBuilder.Entity<BigBuyProduct>(entity =>
        {
            entity.ToTable("bigbuyproducts","kickflip");
            entity.Property(e => e.Manufacturer).HasColumnName("manufacturer");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Sku).HasColumnName("sku");
            entity.Property(e => e.Ean13).HasColumnName("ean13");
            entity.Property(e => e.Weight).HasColumnName("weight");
            entity.Property(e => e.Height).HasColumnName("height");
            entity.Property(e => e.Width).HasColumnName("width");
            entity.Property(e => e.Depth).HasColumnName("depth");
            entity.Property(e => e.Category).HasColumnName("category");
            entity.Property(e => e.WholesalePrice).HasColumnName("wholesaleprice");
            entity.Property(e => e.RetailPrice).HasColumnName("retailprice");
            entity.Property(e => e.Taxonomy).HasColumnName("taxonomy");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.TaxRate).HasColumnName("taxrate");
            entity.Property(e => e.TaxId).HasColumnName("taxid");
            entity.Property(e => e.InShopsPrice).HasColumnName("inshopsprice");
            entity.Property(e => e.Condition).HasColumnName("condition");
            entity.Property(e => e.LogisticClass).HasColumnName("logisticsclass");
        });


        OnModelCreatingPartial(modelBuilder);

    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}