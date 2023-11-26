namespace NDE_Digital_Market.Model.OrderModel
{
    public class SellerOrderDetails
    {
        public int OrderMasterId { get; set; }
        public int OrderDetailId { get; set; }
        public int GoodsId { get; set; }
        public int Quantity { get; set; }
        public decimal Discount { get; set; }
        public decimal Price { get; set; }
        public string DeliveryDate { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public string OrderNo { get; set; }
        //public string FullName { get; set; }
        public string GoodsName { get; set; }
    }
}
