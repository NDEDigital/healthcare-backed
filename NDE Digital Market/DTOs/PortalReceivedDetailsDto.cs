namespace NDE_Digital_Market.DTOs
{
    public class PortalReceivedDetailsDto
    {
        public int? PortalReceivedId { get; set; }
        public int? ProductGroupId { get; set; }
        public int? ProductId { get; set; }
        public string? Specification { get; set; }
        public int? ReceivedQty { get; set; }
        public int? UnitId { get; set; }
        public decimal? Price { get; set; }
        public decimal? TotalPrice { get; set; }
        public int? UserId { get; set; }
        public string? Remarks { get; set; }
        //public string? ApprovedBy { get; set; }
        //public DateTime? ApproveDate { get; set; }
        //public string? ApproveStatus { get; set; }
        public string? AddedBy { get; set; }
        public DateTime? DateAdded { get; set; }
        public string? AddedPC { get; set; }
    }
}
