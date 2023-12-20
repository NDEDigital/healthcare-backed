namespace NDE_Digital_Market.DTOs
{
    public class CompanyDto
    {
        public string? CompanyName { get; set; }
        public string? CompanyAdminCode { get; set; }
        public string? CompanyImage { get; set; } // Assume byte[] for image data
        public DateTime? CompanyFoundationDate { get; set; } // Nullable for potentially missing values
        public string? BusinessRegistrationNumber { get; set; }
        public string? TaxIdentificationNumber { get; set; }
        public string? TradeLicense { get; set; }
        public Boolean? IsActive { get; set; }
        public int? PreferredPaymentMethodID { get; set; }
        public int? BankNameID { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountHolderName { get; set; }
        public string? AddedBy { get; set; }
        public DateTime? DateAdded { get; set; }
        public string? AddedPC { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; } // Nullable for potentially missing values
        public string? UpdatedPC { get; set; }
    }
}
