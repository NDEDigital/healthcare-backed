namespace NDE_Digital_Market.DTOs
{
    public class GetSellerOrderBasedOnUserCodeDto
    {
        public string? OrderNo { get; set; }
        public string? Address { get; set; }
        public int? BUserId { get; set; }
        public string? BuyerName { get; set; }
        public int? ProductGroupID { get; set; }
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? Specification { get; set; }
        public decimal? StockQty { get; set; }
        public decimal? TotalQty { get; set; }
        public int? SaleQty { get; set; }
        public int? UnitId { get; set; }
        public string? Unit { get; set; }
        public decimal? NetPrice { get; set; }
    }
}
