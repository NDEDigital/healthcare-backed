namespace NDE_Digital_Market.Model
{
    public class SellerInvoice
    {

        public string SelesPerson { get; set; }
        public string Company { get; set; }
        public string SelesAddress { get; set; }
        public string SellerContact { get; set; }

        public List<BuyerDetails> BuyerDetailsList { get; set; } = new List<BuyerDetails>();

        public List<SellerInvoiceDetails> SellerInvoiceDetailList { get; set; } = new List<SellerInvoiceDetails>();
  
    }

    public class BuyerDetails
    {
        public string BuyerName { get; set; }
        public string BuyerAddress { get; set; }
        public string BuyerContact { get; set; }

    }

    public class SellerInvoiceDetails
    {

        public string OrderNo { get; set; }
        public string ProductGroupName { get; set; }
        public string ProductName { get; set; }
        public string Specification { get; set; }
        public int Qty { get; set; }
        public string Unit { get; set; }
        public decimal NetPrice { get; set; }

    }
}
