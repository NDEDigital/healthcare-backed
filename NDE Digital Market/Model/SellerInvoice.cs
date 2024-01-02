namespace NDE_Digital_Market.Model
{
    public class SellerInvoice
    {
        public string SSMCode { get; set; }
        public DateTime SSMDate { get; set; }
        public string SelesPerson { get; set; }
        public string Company { get; set; }
        public string SelesAddress { get; set; }
        public string Phone { get; set; }
        public string Challan { get; set; }
        public string Remarks { get; set; }


        public List<SellerInvoiceDetails> SellerInvoiceDetailList { get; set; } = new List<SellerInvoiceDetails>();
    }
    public class SellerInvoiceDetails
    {

        public string OrderNo { get; set; }
        public string ProductGroupName { get; set; }
        public string ProductName { get; set; }
        public string Specification { get; set; }
        public decimal StockQty { get; set; }
        public int SaleQty { get; set; }
        public string Unit { get; set; }
        public decimal NetPrice { get; set; }
        public string SSLRemarks { get; set; }
        public string BuyerName { get; set; }
        public string BuyerPhone { get; set; }
        public string Address { get; set; }

    }
}
