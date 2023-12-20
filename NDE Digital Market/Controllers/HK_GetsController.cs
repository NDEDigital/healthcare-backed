using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NDE_Digital_Market.Services.HK_GetsServices;
using NDE_Digital_Market.Model;

namespace NDE_Digital_Market.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HK_GetsController : ControllerBase
    {
        private readonly IHK_Gets _HKGets;
        public HK_GetsController(IHK_Gets hK_Gets)
        {
            this._HKGets = hK_Gets;
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
                return Ok(res);
            }
            else
            {
                return BadRequest("No Payment method found.");
            }
        }
    }
}
