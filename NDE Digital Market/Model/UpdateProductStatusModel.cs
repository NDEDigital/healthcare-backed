namespace NDE_Digital_Market.Model
{
    public class UpdateProductStatusModel
    {
        public string? userCode { get; set; }
        public string? productIDs { get; set; }
        public string? status { get; set; }
        public string? statusBefore { get; set; }
        public string? updatedPC { get; set; }

    }
}
