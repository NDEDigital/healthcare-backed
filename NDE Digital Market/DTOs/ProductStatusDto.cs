namespace NDE_Digital_Market.DTOs
{
    public class ProductStatusDto
    {
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public int? UserId { get; set; }
        public string? FullName { get; set; }
        public decimal? Price { get; set; }
        public decimal? DiscountAmount { get; set; }
        public string? DiscountPct { get; set; }
        public DateTime? EffectivateDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string? ImagePath { get; set; }
        public decimal? TotalPrice { get; set; }

        public string? CompanyCode { get; set; }
        public string? CompanyName { get; set; }
    }
}
