namespace NDE_Digital_Market.DTOs
{
    public class OrderMasterDataForBuyerDto
    {
        public int? OrderMasterId { get; set; }
        public string? OrderNo { get; set; }
        public DateTime? OrderDate { get; set; }



        public List<OrderDetailsDataForBuyerDto>? OrderDetailsListForBuyer { get; set; }


        public OrderMasterDataForBuyerDto()
        {
            OrderDetailsListForBuyer = new List<OrderDetailsDataForBuyerDto>();
        }
    }
}
