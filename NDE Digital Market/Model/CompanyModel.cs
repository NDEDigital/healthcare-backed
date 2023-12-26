namespace NDE_Digital_Market.Model
{
    public class CompanyModel
    {
        public int? CompanyID { get; set; }
        public int? MaxUser { get; set; }
        public string? CompanyCode { get; set; }
        public string? CompanyName { get; set; }
        public string? Email { get; set; }
        public int? CompanyAdminId { get; set; }
        public IFormFile? CompanyImageFile { get; set; }
        public byte[]? CompanyImageFileBite { get; set; }
        public string? CompanyImage { get; set; } // Assume byte[] for image data
        public DateTime? CompanyFoundationDate { get; set; } // Nullable for potentially missing values
        public string? BusinessRegistrationNumber { get; set; }
        public string? TaxIdentificationNumber { get; set; }
        public IFormFile? TradeLicenseFile { get; set; }
        public byte[]? TradeLicenseFileBite { get; set; }
        public string? TradeLicense { get; set; }
        public int? IsActive { get; set; }
        public int? PreferredPaymentMethodID { get; set; }
        public string? PreferredPaymentMethodName { get; set; }
        public int? BankNameID { get; set; }
        public string? BankName { get; set; }
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
