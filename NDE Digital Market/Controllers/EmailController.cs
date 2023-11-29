using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MailKit.Net.Smtp;
using NDE_Digital_Market.Model;
using NDE_Digital_Market.SharedServices;
using System.Data.SqlClient;
using System.Data;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace NDE_Digital_Market.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public EmailController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpPost]
        [Route("sendEmail")]
        public string SendEmail([FromBody] EmailRequest emailRequest)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("NDE Digital Market", "nde.digital@ndesteel.com"));
                message.To.Add(new MailboxAddress("", emailRequest.To));
                message.Subject = emailRequest.Subject;
                message.Body = new TextPart("plain")
                {
                    Text = emailRequest.Body
                };

                using (var client = new SmtpClient())
                {
                    client.Connect(_configuration["SmtpSettings:Host"], int.Parse(_configuration["SmtpSettings:Port"]), MailKit.Security.SecureSocketOptions.StartTls);
                    client.Authenticate(_configuration["SmtpSettings:Email"], _configuration["SmtpSettings:Password"]);
                    client.Send(message);
                    client.Disconnect(true);
                }

             
            }
            catch (System.Exception ex)
            {
                return "error from server";
            }
            return "success";
        }
        //[HttpGet]
        //[Route("getEmailInfo")]
        //public IActionResult getEmailAddress(string userCode)
        //{
        //    UserModel user = new UserModel();
        //    SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        //    SqlCommand cmd = new SqlCommand("SELECT * FROM User_Registration WHERE user_code = @userCode ", con);
        //    cmd.CommandType = CommandType.Text;
        //    cmd.Parameters.AddWithValue("@userCode", userCode);

        //    con.Open();
        //    SqlDataReader reader = cmd.ExecuteReader();
        //    if (reader.Read())
        //    {
        //        user.UserID = (int)reader["user_id"];
        //        user.UserCode = reader["user_code"].ToString();
        //        user.CounteryRegion = reader["country_region"].ToString();
        //        user.IsBuyer = (bool)reader["is_buyer"];
        //        user.IsSeller = (bool)reader["is_seller"];
        //        user.IsAdmin = (bool)reader["is_admin"];
        //        user.FullName = reader["full_name"].ToString();
        //        user.PhoneNumber = reader["phone_number"].ToString();
        //        user.Email = reader["email"].ToString();
        //        user.Address = reader["address"].ToString();
        //        user.CompanyName = reader["company_name"].ToString();
        //        user.Website = reader["website"].ToString();
        //        user.ProductCategory = reader["product_category"].ToString();
        //        user.YearsInBusiness = reader["years_in_business"].ToString();
        //        user.BusinessRegistrationNumber = reader["business_registration_number"].ToString();
        //        user.TaxIDNumber = reader["tax_id_number"].ToString();
        //        user.PreferredPaymentMethod = reader["preferred_payment_method"].ToString();

        //        con.Close();

        //        // Return the user object as a response
        //        return Ok(new { message = "Got single user data successful", user });
        //    }
        //    else
        //    {
        //        con.Close();
        //        return BadRequest(new { message = "Invalid Inforamtion" });
        //    }
          
        //}
    }
    public class EmailRequest
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
