namespace NDE_Digital_Market.DTOs
{
    public class ProductSearchDto
    {
        public string? CompanyName { get; set; }
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public int? ProductGroupID { get; set; }
        public string? ProductGroupName { get; set; }
        public string? Specification { get; set; }
        public int? UnitId { get; set; }
        public string? Unit { get; set; }
        public decimal? Price { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? DiscountPct { get; set; }
        public string? ImagePath { get; set; }
        public decimal? TotalPrice { get; set; }
        public int? SellerId { get; set; }
        public decimal? AvailableQty { get; set; }
        public int? TotalCount { get; set; }
    }
}
