using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NDE_Digital_Market.Services.HK_GetsServices;
using NDE_Digital_Market.Model;
using System.Data.SqlClient;

namespace NDE_Digital_Market.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HK_GetsController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly SqlConnection con;
        private readonly IHK_Gets _HKGets;
        public HK_GetsController(IConfiguration configuration, IHK_Gets hK_Gets)
        {
            this._HKGets = hK_Gets;
            _configuration = configuration;
            con = new SqlConnection(_configuration.GetConnectionString("HealthCare"));
        }

        [HttpGet("PreferredPaymentMethods")]
        public async Task<IActionResult> PaymentMethodGetAsync()
        {
            List<PaymentMethodModel> res = await _HKGets.PaymentMethodGetAsync();
            if (res.Count > 0)
            {
                return Ok(res);
            }
            else
            {
                return BadRequest("No Payment method found.");
            }
        }

        [HttpGet("PreferredBankNames")]
        public async Task<IActionResult> BankNameGetAsync(int preferredPM)
        {
            List<PaymentMethodModel> res = await _HKGets.BankNameGetAsync(preferredPM);
            if (res.Count > 0)
            {
                return Ok(new { message = res });
            }
            else
            {
                return BadRequest(new { message = "No Payment method found." });
            }
        }

        [HttpGet]
        [Route("GetUnitList")]
        public async Task<List<UnitModel>> GetUnitListAsync()
        {
            
            List<UnitModel> lst = new List<UnitModel>();
            await con.OpenAsync();
            string query = "select UnitId, Name from Units;";

            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        UnitModel modelObj = new UnitModel();
                        modelObj.UnitId = Convert.ToInt32(reader["UnitId"]);
                        modelObj.Name = reader["Name"].ToString();

                        lst.Add(modelObj);
                    }
                }
            }

            return lst;
        }
    }
}
