namespace NDE_Digital_Market.DTOs
{
    public class CreateUserDto
    {
      
        public bool? IsBuyer { get; set; }
        public bool? IsSeller { get; set; }
        public bool? IsAdmin { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Address { get; set; }
        public string CompanyId { get; set; }
     

    }
}
