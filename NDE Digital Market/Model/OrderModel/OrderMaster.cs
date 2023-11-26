namespace NDE_Digital_Market.Model.OrderModel
{
    public class OrderMaster
    {
      
        public string OrderNo { get; set; }
        public string OrderDate { get; set; }
        public string BuyerCode { get; set; }
        public string Address { get; set; }
        public string PaymentMethod { get; set; }
        public int NumberOfItem { get; set; }
        public float TotalPrice { get; set; }
        public string Status { get; set; }
        public string phoneNumber { get; set; }
        public float DeliveryCharge { get; set; }

        
    }
}
