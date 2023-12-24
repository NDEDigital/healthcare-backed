namespace NDE_Digital_Market.DTOs
{
    public class OrderDetailStatusDto
    {
        public int? OrderDetailId { get; set; }
        public int? OrderMasterId { get; set; }
        public int? UserId { get; set; }
        public int? ProductId { get; set; }
        public string? ProductGroupCode { get; set; }
        public string? FullName { get; set; }
        public string? ProductName { get; set; }
        public string? Specification { get; set; }
        public string? Unit { get; set; }
        public string? Status { get; set; }
        public int? Qty { get; set; }
        public int? UnitId { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? Price { get; set; }
        public decimal? DeliveryCharge { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public decimal? DiscountPct { get; set; }
        public decimal? NetPrice { get; set; }
    }
}
