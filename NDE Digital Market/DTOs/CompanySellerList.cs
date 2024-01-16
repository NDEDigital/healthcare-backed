namespace NDE_Digital_Market.DTOs
{
    public class CompanySellerList
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }

        public string Address  { get; set; }
        public DateTime AddedDate { get; set; }
        public bool IsActive { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyName { get; set;}


    }
}
