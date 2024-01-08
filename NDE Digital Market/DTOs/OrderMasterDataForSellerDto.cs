namespace NDE_Digital_Market.DTOs
{
    public class OrderMasterDataForSellerDto
    {
        public int? OrderMasterId { get; set; }
        public string? OrderNo { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal? TotalPrice { get; set; } = decimal.Zero;
        public decimal? TotalDeliveryCharge { get; set; } = decimal.Zero;
        public decimal? TotalAmount { get; set; } = decimal.Zero;

        public List<OrderDetailsDataForSellerDto>? OrderDetailsListForSeller { get; set; }


        public OrderMasterDataForSellerDto()
        {
            OrderDetailsListForSeller = new List<OrderDetailsDataForSellerDto>();
        }
    }
}
