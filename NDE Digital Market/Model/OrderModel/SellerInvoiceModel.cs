namespace NDE_Digital_Market.Model.OrderModel
{
    public class SellerInvoiceModel
    {
        public SellerInvoiceModel()
        {
            // Initialize productDetailsList as an empty list in the constructor
            ProductDetailsList = new List<ProductDetails>();
        }
        public string InvoiceNumber { get; set; }
        public string OrderNo { get; set; }
        public string GenerateDate { get; set; }
        public string OrderDate { get; set; }
        public string DeliveryDate { get; set; }
        public float TotalPrice { get; set; }
        public string SellerName { get; set; }
        public string SellerCompanyName { get; set; }
        public string SellerAddress { get; set; }
        public string SellerPhone { get; set; }
        public string BuyerName { get; set; }
        //  private string BuyerCompanyName { get; set; }
        public string BuyerAddress { get; set; }
        public string BuyerPhone { get; set; }
        public List<ProductDetails> ProductDetailsList { get; set; }
    }
    public class ProductDetails
    {
        public string ProductName { get; set; }
        public string Specification { get; set; }
        public int Quantity { get; set; }
        public float Price { get; set; }
        public float Discount { get; set; }
        public float DeliveryCharge { get; set; }
        public float SubTotalPrice { get; set; }
        public string Status {  get; set; }
    }
}
