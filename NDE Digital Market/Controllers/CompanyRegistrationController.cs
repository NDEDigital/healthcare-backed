using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NDE_Digital_Market.Model;
using NDE_Digital_Market.DTOs;
using NDE_Digital_Market.Services.CompanyRegistrationServices;
using Newtonsoft.Json;

namespace NDE_Digital_Market.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyRegistrationController : ControllerBase
    {
        private readonly ICompanyRegistration _CompanyRegistration;
        public CompanyRegistrationController(ICompanyRegistration companyRegistration)
        {
            this._CompanyRegistration = companyRegistration;
        }


        [HttpPost("Companyexists")]
        public async Task<IActionResult> CompanyexistsCheckAsync(CompanyDto companyDto)
        {
            var res = await _CompanyRegistration.CompanyexistsCheckAsync(companyDto);
            return Ok(res);
        }

        [HttpPost("CreateCompany")]
        public async Task<IActionResult> CompanyRegistrationPostAsync([FromForm] CompanyDto companyDto)
        {
            var res = await _CompanyRegistration.CompanyRegistrationPostAsync(companyDto);
            return Ok(res);
        }

        [HttpGet("GetCompanies")]
        public async Task<IActionResult> GetCompaniesAsync()
        {
            var res = await _CompanyRegistration.GetCompaniesAsync();
            return Ok(res);
        }

        [HttpPut("UpdateCompany")]
        public async Task<IActionResult> UpdateCompany(CompanyDto companyDto)
        {
            //CompanyModel companyModel = JsonConvert.DeserializeObject<CompanyModel>(data);
            var res = await _CompanyRegistration.UpdateCompanyAsync(companyDto);
            return Ok(res);
        }

    }
}
