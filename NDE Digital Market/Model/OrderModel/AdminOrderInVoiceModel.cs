namespace NDE_Digital_Market.Model.OrderModel
{
    public class AdminOrderInVoiceModel
    {
        public AdminOrderInVoiceModel()
        {
            // Initialize productDetailsList as an empty list in the constructor
            ProductDetailsList = new List<AdminProductDetails>();
            // Initialize productDetailsList as an empty list in the constructor
            SellerDetailsList = new List<AdminSellerDetails>();
        }

        public string OrderNo { get; set; }
        public string OrderDate { get; set; }
        public string BuyerName { get; set; }
        public string TotalPrice { get; set; }
        //  private string BuyerCompanyName { get; set; }
        public string BuyerAddress { get; set; }
        public string BuyerPhone { get; set; }
        public List<AdminProductDetails> ProductDetailsList { get; set; }
        public List<AdminSellerDetails> SellerDetailsList { get; set; }
    }
    public class AdminProductDetails
    {
        public string ProductName { get; set; }
        public string Specification { get; set; }
        public int Quantity { get; set; }
        public float Price { get; set; }
        public float Discount { get; set; }
        public float DeliveryCharge { get; set; }
        public float SubTotalPrice { get; set; }
        public int OrderDetailId { get; set; }
        public string SellerCode { get; set; }

    }

    public class AdminSellerDetails
    {
        public string InvoiceNumber { get; set; }
        public string GenerateDate { get; set; }
        public string DeliveryDate { get; set; }

        public string SellerName { get; set; }
        public string SellerCode { get; set; }
        public string SellerCompanyName { get; set; }
        public string SellerAddress { get; set; }
        public string SellerPhone { get; set; }
        public int OrderDetailId { get; set; }
    }

}

