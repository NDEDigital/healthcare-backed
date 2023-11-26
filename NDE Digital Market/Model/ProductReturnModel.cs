namespace NDE_Digital_Market.Model
{
    public class ProductReturnModel
    {
        public int? ReturnId { get; set; }
        public string? GroupName { get; set; }
        public string? GroupCode { get; set; }
        public int? GoodsId { get; set; }
        public string? Remarks { get; set; }
        public int? TypeId { get; set; }
        public double? Price { get; set; }

        public int? DetailsId { get; set; }
        public int? totalRowsCount { get; set; } // for admin
        public int? ToReturnCount { get; set; }
        public int? TotalRowCount { get; set; } // for seller
        
        public int? ReturnedCount { get; set; }
        public DateTime ApplyDate { get; set; }      
        public string? SellerCode { get; set; }
        public string? ReturnType { get; set; }
        public string? Status { get; set; }
        public string? GoodsName { get; set; }
        
        public string? OrderNo { get; set; }
        public DateTime DeliveryDate { get; set; }



    }
}
