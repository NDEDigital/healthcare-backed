namespace NDE_Digital_Market.DTOs
{
    public class GetOrderInvoiceByMasterIdDto
    {
        // Buyer details
        public string InvoiceNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string BuyerName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string PaymentMethod { get; set; }
        public int NumberOfItem { get; set; }
        public decimal TotalPrice { get; set; }

        public List<OrderInvoiceDetails> OrderInvoiceDetailList { get; set; }

        public GetOrderInvoiceByMasterIdDto()
        {
            OrderInvoiceDetailList = new List<OrderInvoiceDetails>();
        }
    }

    public class OrderInvoiceDetails
    {
        // OrderInvoice details
        public string ProductName { get; set; }
        public string Status { get; set; }
        public string Specification { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; }
        public decimal Price { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public decimal DeliveryCharge { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountPct { get; set; }
        public decimal NetPrice { get; set; }
        public decimal DetailDeliveryCharge { get; set; }
        public decimal SubTotalPrice { get; set; }
        public string SelesPerson { get; set; }
        public int SellerId { get; set; }
        public string SelesAddress { get; set; }
        public string SellerContact { get; set; }
        public string Company { get; set; }
    }
}
