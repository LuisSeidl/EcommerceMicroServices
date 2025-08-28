using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCore.Entities
{
    public partial class CompetitorProduct
    {
        public string url { get; set; }
        
        [Key]
        public string? ebayId { get; set; }
        public string? sku { get; set; }
        public string? ean13 { get; set; }
        public string? sellerTitle { get; set; }
        public string? sellerName { get; set; }
        public Decimal? sellerPrice { get; set; }
        public bool? alreadyRead {  get; set; }
    }
}
