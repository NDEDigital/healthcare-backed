namespace NDE_Digital_Market.DTOs
{
    public class ProductReturnDto
    {
        //public int? ProductReturnId { get; set; }
        //public string? ProductReturnCode { get; set; }
        public int? ProductGroupId { get; set; }
        public int? ProductId { get; set; }
        public string? OrderNo { get; set; }
        public decimal? Price { get; set; }
        public int? OrderDetailsId { get; set; }
        public int? SellerId { get; set; }
        public DateTime? ApplyDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string? Remarks { get; set; }
        public DateTime? AddedDate { get; set; }
        //public DateTime? UpdatedDate { get; set; }
        public string? AddedBy { get; set; }
        //public string? UpdatedBy { get; set; }
        public string? AddedPc { get; set; }
        //public string? UpdatedPc { get; set; }
    }
}
