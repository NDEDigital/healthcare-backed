namespace NDE_Digital_Market.Model
{
    public class UserModel
    {
        public string CompanyCode { get; set; }
        public int UserId { get; set; }
        public string UserCode { get; set; }
        public bool? IsBuyer { get; set; }
        public bool? IsSeller { get; set; }
        public bool? IsAdmin { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public string Address { get; set; }
        public DateTime? TimeStamp { get; set; }
        public int IsActive { get; set; }
        public DateTime? AddedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string AddedBy { get; set; }
        public string AddedPc { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedPc { get; set; }
    }
}
