namespace NDE_Digital_Market.Model
{
    public class ProductListModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int ProductGroupID { get; set; }
        public string Specification { get; set; }
        public int UnitId { get; set; }
        public bool IsActive { get; set; }
        public DateTime AddedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string AddedBy { get; set; }
        public string UpdatedBy { get; set; }
        public string AddedPc { get; set; }
        public string UpdatedPc { get; set; }
        public string ImagePath { get; set; }
        public int Status { get; set; }
        public string ProductSubName { get; set; }
    }
}
