namespace NDE_Digital_Market.DTOs
{
    public class ProductListDto
    {
        public string? ProductName { get; set; }
        public int? ProductGroupID { get; set; }
        public string? Specification { get; set; }
        public int? UnitId { get; set; }
        public IFormFile? ImageFile { get; set; }
        public string? ProductSubName { get; set; }
        public string? AddedBy { get; set; }
        public string? AddedPC { get; set; }

    }
}
