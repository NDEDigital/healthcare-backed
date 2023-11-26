namespace NDE_Digital_Market.Model.OrderModel
{
    public class SellerOrderMaster
    {
        public int OrderMasterId { get; set; }
        public string OrderNo { get; set; }
        public string OrderDate { get; set; }
        public string Address { get; set; }
        public string PaymentMethod { get; set; }
        public int NumberofItem { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public int TotalRowCount { get; set; }
        public List<SellerOrderDetails> sellerOrderDetails { get; set; }
    }
}
