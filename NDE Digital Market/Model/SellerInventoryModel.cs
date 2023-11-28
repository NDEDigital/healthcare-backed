using System.Text.RegularExpressions;

namespace NDE_Digital_Market.Model
{
    public class SellerInventoryModel
    {
        public int  GoodsID { get; set; }
        public string GoodsName { get; set; }
        public string GroupCode { get; set; }
        public string GroupName { get; set; }
        public int AvailableQuantity { get; set; }
        public int TotalQuantity { get; set; }
        public int salesQuantiy { get; set; }
        public int Price { get; set; }

    

    }
}
