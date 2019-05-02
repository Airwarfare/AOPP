using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albion
{
    class MarketplaceOrder
    {
        public Int64 Id { get; set; }
        public Int64 UnitPriceSilver { get; set; }
        public Int64 TotalPriceSilver { get; set; }
        public int Amount { get; set; }
        public int Tier { get; set; }
        public bool IsFinished { get; set; }
        public string AuctionType { get; set; }
        public bool HasBuyerFetched { get; set; }
        public bool HasSellerFetched { get; set; }
        public string SellerCharacterId { get; set; }
        public string SellerName { get; set; }
        public object BuyerCharacterId { get; set; }
        public object BuyerName { get; set; }
        public string ItemTypeId { get; set; }
        public string ItemGroupTypeId { get; set; }
        public int EnchantmentLevel { get; set; }
        public int QualityLevel { get; set; }
        public DateTime Expires { get; set; }

    }
}
