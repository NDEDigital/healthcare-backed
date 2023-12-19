namespace NDE_Digital_Market.DTOs
{
    public class CreateUserDto
    {
        public string CountryRegion { get; set; }
        public bool? IsBuyer { get; set; }
        public bool? IsSeller { get; set; }
        public bool? IsAdmin { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Address { get; set; }
        public string CompanyName { get; set; }
        public string Website { get; set; }
        public string ProductCategory { get; set; }
        public DateTime? CompanyFoundedDate { get; set; }
        public string BusinessRegistrationNumber { get; set; }
        public string TaxIdNumber { get; set; }
        public int? PaymentMethodId { get; set; }
        public string AccountNo { get; set; }
        public string AccountHolderName { get; set; }
        public int? BankId { get; set; }
        public int? MobileBankingTypeId { get; set; }
        public string MobileBankingNo { get; set; }

    }
}
