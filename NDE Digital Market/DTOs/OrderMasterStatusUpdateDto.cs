using NDE_Digital_Market.DTOs;
namespace NDE_Digital_Market.DTOs
{
    public class OrderMasterStatusUpdateDto
    {
        public int OrderMasterId { get; set; }
        public string Status { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedPC { get; set; }
        public List<OrderDetailsStatusUpdateDto>? OrderDetailsStatusUpdatelist { get; set; }
        public OrderMasterStatusUpdateDto()
        {
            OrderDetailsStatusUpdatelist = new List<OrderDetailsStatusUpdateDto>();
        }
    }
}
