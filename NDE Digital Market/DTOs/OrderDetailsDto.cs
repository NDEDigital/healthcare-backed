namespace NDE_Digital_Market.DTOs
{
    public class OrderDetailsDto
    {
        public int? OrderMasterId { get; set; }
        public int? UserId { get; set; }
        public int? ProductId { get; set; }
        public string? ProductGroupCode { get; set; }
        public string? Specification { get; set; }
        public int? Qty { get; set; }
        public int? UnitId { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? Price { get; set; }
        public decimal? DeliveryCharge { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public decimal? DiscountPct { get; set; }
        public decimal? NetPrice { get; set; }
        //public DateTime? AddedDate { get; set; }
        public string? AddedBy { get; set; }
        public string? AddedPC { get; set; }
    }
}
