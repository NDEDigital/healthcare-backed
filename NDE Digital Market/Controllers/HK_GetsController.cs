﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NDE_Digital_Market.Services.HK_GetsServices;
using NDE_Digital_Market.Model;
using System.Data.SqlClient;
using NDE_Digital_Market.SharedServices;
using System.Data;

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
            CommonServices commonServices = new CommonServices(configuration);
            this._HKGets = hK_Gets;
            _configuration = configuration;
            con = new SqlConnection(commonServices.HealthCareConnection);
        }

        [HttpGet("PreferredPaymentMethods")]
        public async Task<IActionResult> PaymentMethodGetAsync()
        {
            try
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
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while fetching payment methods.");
            }
        }


        [HttpGet("PreferredBankNames")]
        public async Task<IActionResult> BankNameGetAsync(int preferredPM)
        {
            try
            {
                List<PaymentMethodModel> res = await _HKGets.BankNameGetAsync(preferredPM);

                if (res.Count > 0)
                {
                    return Ok(res);
                }
                else
                {
                    return BadRequest(new { message = "No Payment method found." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching bank names." });
            }
        }

        [HttpGet]
        [Route("GetUnitList")]
        public async Task<List<UnitModel>> GetUnitListAsync()
        {
            try
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
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }

        [HttpGet("GetReturnList")]
        public async Task<IActionResult> GetReturnListAsync()
        {
            try
            {
                List<ReturnTypeModel> lst = new List<ReturnTypeModel>();
                await con.OpenAsync();
                string query = "select ReturnTypeId, ReturnTypeName from HK_ReturnType;";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            ReturnTypeModel modelObj = new ReturnTypeModel();
                            modelObj.ReturnTypeId = Convert.ToInt32(reader["ReturnTypeId"]);
                            modelObj.ReturnTypeName = reader["ReturnTypeName"].ToString();

                            lst.Add(modelObj);
                        }
                    }
                }

                return Ok(lst);
            }

            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred while fetching ReturnTypes." });
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }



    }
}
