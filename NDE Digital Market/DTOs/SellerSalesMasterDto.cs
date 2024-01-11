namespace NDE_Digital_Market.DTOs
{
    public class SellerSalesMasterDto
    {
        //public int? SSMId { get; set; }
        //public string? SSMCode { get; set; }
        public DateTime? SSMDate { get; set; }
        public int? UserId { get; set; }
        public decimal? TotalPrice { get; set; }
        public string? Challan { get; set; }
        public string? Remarks { get; set; }
        public int? BUserId { get; set; }
        public int? AddedBy { get; set; }
        public DateTime? DateAdded { get; set; }
        public string? AddedPC { get; set; }
        public string? CompanyCode { get; set; }

        public List<SellerSalesDetailDto>? SellerSalesDetailsList { get; set; }

        public SellerSalesMasterDto()
        {
            SellerSalesDetailsList = new List<SellerSalesDetailDto>();
        }
    }
}
