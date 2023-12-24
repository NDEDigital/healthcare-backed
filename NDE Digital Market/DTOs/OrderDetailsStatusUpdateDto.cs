namespace NDE_Digital_Market.DTOs
{
    public class OrderDetailsStatusUpdateDto
    {
        public int OrderMasterId { get; set; }
        public int OrderDetailId { get; set; }
        public string Status { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string AddedPC { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedPC { get; set; }
    }
}
