namespace NDE_Digital_Market.Model.OrderModel
{
    public class OrderDetail
    {
      
        public int GoodsId { get; set; }
        public string GoodsName { get; set; }
        public int Quantity { get; set; }
        public float Discount { get; set; }
        public float Price { get; set; }
        public string DeliveryDate { get; set; }
        public float DeliveryCharge { get; set; }
        public string Specification { get; set; }
        public string GroupCode { get; set; }
        public string? SellerCode  { get; set; }
        public string Status { get; set; }
     }
}
