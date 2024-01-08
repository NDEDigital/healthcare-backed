namespace NDE_Digital_Market.DTOs
{
    public class ProductGroupsDto
    {
        public int? ProductGroupID { get; set; }
        public string? ProductGroupName { get; set; }
        public string? ProductGroupPrefix { get; set; }
        public string? ProductGroupDetails { get; set; }
        public IFormFile? ImageFile { get; set; }
        public string? ExistingImageFileName { get; set; }
        public string? AddedBy { get; set; }
        public DateTime? DateAdded { get; set; }
        public string? AddedPC { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string? UpdatedPC { get; set; }
    }
}
