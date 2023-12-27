namespace NDE_Digital_Market.DTOs
{
    public class SellerSalesDetailDto
    {
        public int? SSMId { get; set; }
        public string? OrderNo { get; set; }
        public int? ProductId { get; set; }
        public string? Specification { get; set; }
        public int? StockQty { get; set; }
        public int? SaleQty { get; set; }
        public int? UnitId { get; set; }
        public decimal? NetPrice { get; set; }
        public string? Remarks { get; set; }
        public string? Address { get; set; }
        public int? ProductGroupID { get; set; }


        public DateTime? AddedDate { get; set; }
        public int? AddedBy { get; set; }
        public string? AddedPC { get; set; }

    }
}
