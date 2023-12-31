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
      
    }
    public class EmailRequest
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
