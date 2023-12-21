namespace NDE_Digital_Market.DTOs
{
    public class OrderDataBaseOnStatusDto
    {
        public int? OrderMasterId { get; set; }
        public string? OrderNo { get; set; }
        public DateTime? OrderDate { get; set; }
        public string? Address { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string? PaymentMethod { get; set; }
        public int? NumberOfItem { get; set; }
        public decimal? TotalPrice { get; set; }
        public string? PhoneNumber { get; set; }
        public decimal? DeliveryCharge { get; set; }
        public string? Status { get; set; }
    }
}
