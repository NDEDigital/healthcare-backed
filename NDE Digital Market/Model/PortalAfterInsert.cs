namespace NDE_Digital_Market.Model
{
    public class PortalAfterInsert
    {
        public int PortalReceivedId { get; set; }
        public string PortalReceivedCode { get; set; }
        public DateTime MaterialReceivedDate { get; set; }
        public string ChallanNo { get; set; }
        public DateTime? ChallanDate { get; set; }
        public string Remarks { get; set; }
        public List<PortalReceivedDetailAfterInsert> PortalReceivedDetailAfterInsertlList { get; set; } = new List<PortalReceivedDetailAfterInsert>();
    }
    public class PortalReceivedDetailAfterInsert
    {
        public int PortalReceivedId { get; set; }
        public int PortalDetailsId { get; set; }
        public int ProductGroupId { get; set; }
        public string ProductGroupName { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Specification { get; set; }
        public decimal ReceivedQty { get; set; }
        public int UnitId { get; set; }
        public string Unit { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal AvailableQty { get; set; }
        public string Remarks { get; set; }
    }
}
