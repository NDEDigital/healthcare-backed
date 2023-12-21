namespace NDE_Digital_Market.DTOs
{
    public class PortalReceivedMasterDto 
    {
        public int? PortalReceivedId { get; set; }
        public string? PortalReceivedCode { get; set; }
        public DateTime? MaterialReceivedDate { get; set; }
        public string? ChallanNo { get; set; }
        public DateTime? ChallanDate { get; set; }
        public string? Remarks { get; set; }
        public int? UserId { get; set; }
        public string? CompanyCode { get; set; }
        //public DateTime AddedDate { get; set; }
        public string? AddedBy { get; set; }
        public string? AddedPC { get; set; }
        public List<PortalReceivedDetailsDto>? PortalReceivedDetailslist { get; set; }

        public PortalReceivedMasterDto()
        {
            PortalReceivedDetailslist = new List<PortalReceivedDetailsDto>();
        }

    }
}
