namespace NDE_Digital_Market.Model
{
    public class GoodsQuantityModel
    {
        public string? CompanyName { get; set; }
        public string? GroupCode { get; set; }
        public string? GoodsId { get; set; }
        public string? GroupName { get; set; }
        public string? GoodsName { get; set; }
        public string? Specification { get; set; }
        public float? ApproveSalesQty { get; set; }
        public string? SellerCode { get; set; }
        public string? QuantityUnit { get; set; }
        public string? ImagePath { get; set; }
        public float Price { get; set; }
        public string? Status { get; set; }
        public DateTime? AddedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? AddedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public string? AddedPc { get; set; }
        public string? UpdatedPc { get; set; }
        public IFormFile? Image { get; set; }
        public string? ImageName { get; set; }
        public int? Quantity { get; set; }
        
    }
}
