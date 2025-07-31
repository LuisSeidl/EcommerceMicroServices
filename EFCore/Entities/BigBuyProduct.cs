using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCore.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class BigBuyProduct
    {
        [JsonPropertyName("manufacturer")]
        public int Manufacturer { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("sku")]
        public string Sku { get; set; }

        [JsonPropertyName("ean13")]
        public string? Ean13 { get; set; }

        [JsonPropertyName("weight")]
        public decimal Weight { get; set; }

        [JsonPropertyName("height")]
        public decimal Height { get; set; }

        [JsonPropertyName("width")]
        public decimal Width { get; set; }

        [JsonPropertyName("depth")]
        public decimal Depth { get; set; }

        [JsonPropertyName("category")]
        public int Category { get; set; }

        [JsonPropertyName("wholesalePrice")]
        public decimal WholesalePrice { get; set; } 

        [JsonPropertyName("retailPrice")]
        public decimal RetailPrice { get; set; }

        [JsonPropertyName("taxonomy")]
        public int Taxonomy { get; set; }

        [JsonPropertyName("active")]
        public int Active { get; set; }

        [JsonPropertyName("taxRate")]
        public int TaxRate { get; set; }

        [JsonPropertyName("taxId")]
        public int TaxId { get; set; }

        [JsonPropertyName("inShopsPrice")]
        public decimal InShopsPrice { get; set; }

        [JsonPropertyName("condition")]
        public string? Condition { get; set; }

        [JsonPropertyName("logisticClass")]
        public string? LogisticClass { get; set; }


        public void Update(BigBuyProduct other) {

            if (other == null) throw new ArgumentNullException(nameof(other));

            Manufacturer = other.Manufacturer;
            
            Weight = other.Weight;
            Height = other.Height;
            Width = other.Width;
            Depth = other.Depth;
            Category = other.Category;
            WholesalePrice = other.WholesalePrice;
            RetailPrice = other.RetailPrice;
            Taxonomy = other.Taxonomy;
            Active = other.Active;
            TaxRate = other.TaxRate;
            TaxId = other.TaxId;
            InShopsPrice = other.InShopsPrice;
            Condition = other.Condition;
            LogisticClass = other.LogisticClass;
       
        }

    }
}
