namespace NDE_Digital_Market.DTOs
{
    public class OrderDetailsDataForBuyerDto
    {
        public int? OrderDetailId { get; set; }
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ImagePath { get; set; }
        public int? Qty { get; set; }
        public string? Status { get; set; }
    }
}
