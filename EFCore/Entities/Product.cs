using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;

namespace EFCore.Entities
{
    public partial class Product
    {
        public string? sku {  get; set; }
        public string? ean13 { get; set; }
        public string? id { get; set; }
        public string? name { get; set; }
        public string? manufacturer { get; set; }
        public string? category { get; set; }
        public string? condition { get; set; }
        public int? active { get; set; }
        public string? ebayTitle { get; set; }
        public float? wholesalePrice { get; set; }
        public float? recommendedPrice { get; set; }
        public float? retailPrice { get; set; }
        public float? currentPrice { get; set; }
        public float? competitorPrice { get; set; }

        public DateTime? lastUpdated { get; set; }

    }
}
