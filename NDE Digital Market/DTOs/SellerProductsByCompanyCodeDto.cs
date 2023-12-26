namespace NDE_Digital_Market.DTOs
{
    public class SellerProductsByCompanyCodeDto
    {
        public int SellerProductId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountPct { get; set; }
        public DateTime EffectivateDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime AddedDate { get; set; }
        public string ImagePath { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
