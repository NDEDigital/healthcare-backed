namespace NDE_Digital_Market.Model
{
    public class UpdatePasswordModel
    {

        public string userCode { get; set; }
        public string oldPassword { get; set; }
        public string newPassword { get; set; }
    }
}