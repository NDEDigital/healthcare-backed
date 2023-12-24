namespace NDE_Digital_Market.DTOs
{
    public class GetProductListByStatusDto
    {
        public int? ProductId { get; set; }
        public int? UnitId { get; set; }
        public int? ProductGroupID { get; set; }
        public string? ProductName { get; set; }
        public string? Specification { get; set; }
        public string? ImagePath { get; set; }
        public string? ProductSubName { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? AddedDate { get; set; }
    }
}
