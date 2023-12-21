namespace NDE_Digital_Market.DTOs
{
    public class OrderMasterDto
    {
        //public int OrderMasterId { get; set; }
        //public string OrderNo { get; set; }
        public DateTime? OrderDate { get; set; }
        public string? Address { get; set; }
        public int? UserId { get; set; }
        public string? PaymentMethod { get; set; }
        public int? NumberOfItem { get; set; }
        public decimal? TotalPrice { get; set; }
        public string? PhoneNumber { get; set; }
        public decimal? DeliveryCharge { get; set; }
        //public string Status { get; set; }
        //public DateTime AddedDate { get; set; }
        public string? AddedBy { get; set; }
        public string? AddedPC { get; set; }


        public List<OrderDetailsDto>? OrderDetailsList { get; set; }

        public OrderMasterDto()
        {
            OrderDetailsList = new List<OrderDetailsDto>();
        }
    }
}
