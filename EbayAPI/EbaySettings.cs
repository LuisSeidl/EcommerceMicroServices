using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbayAPI
{
    public class EbaySettings
    {
        
        
        public string Secret { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;

        public string DevID {  get; set; } = string.Empty;
        public string SellerToken { get; set; } = string.Empty;

        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    
    }
}
