namespace NDE_Digital_Market.Model.MaterialStock
{
    public class MaterialStockInsert
    {
        public string GroupCode { get; set; }
        public int GoodsId { get; set; }
        public string SellerCode { get; set; }
        public float PreviousQty { get; set; }
        public float PresentQty { get; set; }

    }
}
