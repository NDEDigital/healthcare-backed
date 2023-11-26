namespace NDE_Digital_Market.Model.OrderModel
{
    public class BuyerOrderModel
    {
        public BuyerOrderModel()
        {
        //    // Initialize productDetailsList as an empty list in the constructor
            OrderDetailsList = new List<OrderDetails>();
        }
        public int OrderMasterId { get; set; }
        public string OrderNo { get; set; }
        public string OrderDate { get; set; }
        public float TotalPrice { get; set; }
        public float DeliveryCharge { get; set; }
        public float Subtotal { get; set; }
        public string ShippingAddress { get; set; }
        public string ShippingContact { get; set; }
        public string BillingAddress { get; set; }
        public string BillingContact { get; set; }
        public string BuyerName { get; set; }

        //public string Status { get; set; }
        public List<OrderDetails> OrderDetailsList { get; set; }
    }
    public class OrderDetails
    {
        public int OrderMasterId { get; set; }
        public int orderDetailId { get; set; }
        public string GoodsName { get; set; }
        public int GoodsId { get; set; }
        public string GroupCode { get; set; }
        public int Quantity { get; set; }
        public float Price { get; set; }
        public string Status { get; set; }
        public string DeliveryDate { get; set; }
        public string SellerCode { get; set; }
        public string SellerName { get; set; }
        public string imagePath { get; set; }
        public string GroupName { get; set; }

    }


}
