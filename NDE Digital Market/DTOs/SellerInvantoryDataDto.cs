namespace NDE_Digital_Market.DTOs
{
    public class SellerInvantoryDataDto
    {
        public string ProductName { get; set; }
        public string ProductGroupName { get; set; }
        public string Specification { get; set; }
        public string Unit { get; set; }
        public decimal Price { get; set; }
        public int TotalQty { get; set; }
        public int AvailableQty { get; set; }
        public int SaleQty { get; set; }
    }
}
