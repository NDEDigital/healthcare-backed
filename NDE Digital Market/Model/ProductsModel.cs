namespace NDE_Digital_Market.Model
{
    public class ProductsModel
    {
        public int ProductId { get; set; }
        public string? ProductCode { get; set; }
        public string? ProductName { get; set; }
        public string? ProductDescription { get; set; }
        public string? MaterialType { get; set; }
        public string? MaterialName { get; set; }
        public float Height { get; set; }
        public float Width { get; set; }    
        public float Length { get; set; }   
        public float Weight { get; set; }   
        public string? Finish { get; set; }
        public string? Grade { get; set; }
        public float Price { get; set; }
        public IFormFile? Image { get; set; }
        public string? ImageName { get; set; }
        public string? ImagePath { get; set; }
       
        public string? SupplierCode { get; set; }
        public float? Quantity { get; set; }
        public string? QuantityUnit { get; set; }
        public string? DimensionUnit { get; set; }
        public string? WeightUnit { get; set; }
        public DateTime? AddedDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? AddedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public string? AddedPC { get; set; }
        public string? UpdatedPC { get; set; } 
        public string? Status { get; set; } 
        public int? StatusBit { get; set; } 
 

    }
}
