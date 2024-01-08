namespace NDE_Digital_Market.DTOs
{
    public class OrderDetails2ndMDetailsForBuyerDto
    {
        public string ProductName { get; set; }
        public string? Imagepath { get; set; }
        public decimal? Price { get; set; }
        public decimal? ProductTotalPrice { get; set; }
        public decimal? ProductSubtotal { get; set; }
        public int? TotalQty { get; set; }
        public decimal? DeliveryCharge { get; set; }
    }
}
