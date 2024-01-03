namespace NDE_Digital_Market.DTOs
{
    public class OrderDetails2ndMasterForBuyerDto
    {
        public int? SellerId { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string? Status { get; set; }
        public decimal? PackageSubtotal { get; set; } = decimal.Zero;
        public int? PackageDeliveryCharge { get; set; } = 0;

        public List<OrderDetails2ndMDetailsForBuyerDto>? OrderDetails2ndMDetailsListForBuyer { get; set; }

        public OrderDetails2ndMasterForBuyerDto()
        {
            OrderDetails2ndMDetailsListForBuyer = new List<OrderDetails2ndMDetailsForBuyerDto>();
        }

    }
}
