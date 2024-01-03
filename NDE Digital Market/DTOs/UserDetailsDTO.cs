namespace NDE_Digital_Market.DTOs
{
    public class UserDetailsDTO
    {
        public int? UserId { get; set; }

        public string? UserCode { get; set; }
        public string? FullName { get; set; }
        public bool? IsAdmin { get; set; }
        public bool? IsBuyer { get; set; }
        public bool? IsSeller { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? CompanyName { get; set; }
        public int? YearsInBusiness { get; set; }
        public string? BusinessRegistrationNumber { get; set; }
        public string? TaxIdentificationNumber { get; set; }
        public int? PreferredPaymentMethodID { get; set; }
        public string? PMName { get; set; }
        public int? BankNameID { get; set; }
        public string? PMBankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountHolderName { get; set; }
    }
}
