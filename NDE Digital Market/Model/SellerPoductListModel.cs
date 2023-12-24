namespace NDE_Digital_Market.Model
{
    public class SellerPoductListModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int ProductGroupId { get; set; }
        public string Specification { get; set; }
        public int UnitId { get; set; }
        public string Unit { get; set; }
        public Decimal Price { get; set; }
        public Decimal AvailableQty { get; set; }

    }
}
