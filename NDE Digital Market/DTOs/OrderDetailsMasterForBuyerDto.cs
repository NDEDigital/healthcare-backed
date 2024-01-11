namespace NDE_Digital_Market.DTOs
{
    public class OrderDetailsMasterForBuyerDto
    {
        public int? OrderMasterId { get; set; }
        public string? OrderNo { get; set; }
        public DateTime? OrderDate { get; set; }
        public string? PaymentMethod { get; set; }
        public string? BuyerName { get; set; }
        public string? ShippingAddress { get; set; }
        public string? ShippingPhoneNumber { get; set; }
        public string? BillingAddress { get; set; }
        public string? BillingPhoneNumber { get; set; }
        public decimal? SubTotal { get; set; } = decimal.Zero;
        public decimal? TotalDeliveryCharge { get; set; } = decimal.Zero;
        public decimal? TotalAmount { get; set; } = decimal.Zero;

        public List<OrderDetails2ndMasterForBuyerDto>? OrderDetails2ndMasterListForBuyer { get; set; }

        public OrderDetailsMasterForBuyerDto()
        {
            OrderDetails2ndMasterListForBuyer = new List<OrderDetails2ndMasterForBuyerDto>();
        }

    }
}
