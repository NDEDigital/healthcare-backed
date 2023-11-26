namespace NDE_Digital_Market.Model
{
    public class GoodsQuantityModel
    {
        public string CompanyName { get; set; }
        public string GroupCode { get; set; }
        public string GoodsID { get; set; }
        public string GroupName { get; set; }
        public string GoodsName { get; set; }
        public string Specification { get; set; }
        public string ApproveSalesQty { get; set; } = "0";
        public string SalesQty { get; set; } = "0";
        public string StockQty { get; set; } = "0";
        public string SellerCode { get; set; }
        public string Weight { get; set; } = "0";
        public string Length { get; set; } = "0";
        public string Finish { get; set; } = "0";
        public string Grade { get; set; } = "0";
        public string? QuantityUnit { get; set; }
        public string? DimensionUnit { get; set; }
        public string? WeightUnit { get; set; }
        public string? ImagePath { get; set; }
        public string Price { get; set; } = "";

    }
}
