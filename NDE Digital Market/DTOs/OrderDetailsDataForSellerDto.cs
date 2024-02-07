namespace NDE_Digital_Market.DTOs
{
    public class OrderDetailsDataForSellerDto
    {
        public int? OrderDetailId { get; set; }
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ImagePath { get; set; }
        public int? Qty { get; set; }
        public decimal? Price { get; set; }
        public decimal? DeliveryCharge { get; set; }
        public string? Specification { get; set; }
        public decimal? StockQty { get; set; }
        public int? SaleQty { get; set; }
        public int? UnitId { get; set; }
        public decimal? NetPrice { get; set; }
        public int? ProductGroupID { get; set; }






        public string? Status { get; set; }
        public string? ReturnTypeName { get; set; }
    }
}
