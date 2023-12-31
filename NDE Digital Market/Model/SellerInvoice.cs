namespace NDE_Digital_Market.Model
{
    public class SellerInvoice
    {
        public int SSMId { get; set; }
        public string SSMCode { get; set; }
        public DateTime SSMDate { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public decimal TotalPrice { get; set; }
        public string Challan { get; set; }
        public string Remarks { get; set; }
        public int BUserId { get; set; }
        public string BuyerName { get; set; }
        public string OrderNo { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Specification { get; set; }
        public decimal StockQty { get; set; }
        public int SaleQty { get; set; }
        public int UnitId { get; set; }
        public string Unit { get; set; }
        public decimal NetPrice { get; set; }
        public string SSLRemarks { get; set; }
        public string Address { get; set; }
        public int ProductGroupID { get; set; }
        public string ProductGroupName { get; set; }
    }
}
