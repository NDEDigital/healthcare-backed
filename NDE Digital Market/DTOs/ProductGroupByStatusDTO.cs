namespace NDE_Digital_Market.DTOs
{
    public class ProductGroupByStatusDTO
    {
        public int? ProductGroupID { get; set; }
        public string? ProductGroupCode { get; set; }
        public string? ProductGroupName { get; set; }
        public string? ProductGroupPrefix { get; set; }
        public string? ProductGroupDetails { get; set; }
        public bool IsActive { get; set; }
        public DateTime? DateAdded { get; set; }

    }
}
