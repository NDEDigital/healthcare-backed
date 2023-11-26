namespace NDE_Digital_Market.Model
{
    public class UserModel
    {
        public int? UserID { get; set; }
        public string? UserCode { get; set; }
        public string? CounteryRegion { get; set; }
        //public string? TradeRole {get; set;}
        public bool IsBuyer { get; set; }
        public bool IsSeller { get; set; }
        public bool IsAdmin { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Address { get; set; }
        public string? CompanyName { get; set; }
        public string? Website { get; set; }
        public string? ProductCategory { get; set; }
        public string? YearsInBusiness { get; set; }
        public string? BusinessRegistrationNumber { get; set; }
        public string? TaxIDNumber { get; set; }
        public string? PreferredPaymentMethod { get; set; }
    }
}
