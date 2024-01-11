namespace NDE_Digital_Market.DTOs
{
    public class ProductListDto
    {
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public int? ProductGroupID { get; set; }
        public string? Specification { get; set; }
        public int? UnitId { get; set; }
        public IFormFile? ImageFile { get; set; }
        public string? ExistingImageFileName { get; set; }
        public string? ProductSubName { get; set; }
        public string? AddedBy { get; set; }
        public string? AddedPC { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedPC { get; set; }

    }
}
