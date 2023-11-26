namespace NDE_Digital_Market.Model.OrderModel
{
    public class AdminOrderMaster
    {
        public string OrderMasterId { get; set; }
        public string OrderNo { get; set; }
        public string OrderDate { get; set; }
        public string Address { get; set; }
        public string PaymentMethod { get; set; }
        public string NumberOfItem { get; set; }
        public string TotalPrice { get; set; }
        public string Status { get; set; }
        public int TotalRowsCount { get; set; }
    }
}
