using Microsoft.AspNetCore.Mvc;
using NDE_Digital_Market.SharedServices;
using NDE_Digital_Market.Model;
using NDE_Digital_Market.DTOs;
using System.Data.SqlClient;
using System.Data;

namespace NDE_Digital_Market.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductQuantityController : Controller
    {
        private CommonServices _commonServices;
        private readonly string _healthCareConnection;
        private readonly IConfiguration configuration;
        private readonly SqlConnection con;

        public ProductQuantityController(IConfiguration config)
        {
            _commonServices = new CommonServices(config);
            _healthCareConnection = config.GetConnectionString("HealthCare");
            configuration = config;
            con = new SqlConnection(configuration.GetConnectionString("HealthCare"));
        }


        [HttpGet("GetProductForAddQtyByUserId/{companyCode}")]
        public async Task<IActionResult> GetProductForAddQtyByUserId(string companyCode)
        {
            //string DecryptId = CommonServices.DecryptPassword(companyCode);
            var products = new List<SellerPoductListModel>();

            try
            {
                using (var connection = new SqlConnection(_healthCareConnection))
                {
                    using (var command = new SqlCommand("GetProductForAddQtyByCompanyCode", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@CompanyCode", companyCode));

                        await connection.OpenAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var product = new SellerPoductListModel
                                {
                                    ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                    ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                                    ProductGroupId = reader.GetInt32(reader.GetOrdinal("ProductGroupID")),
                                    Specification = reader.IsDBNull(reader.GetOrdinal("Specification")) ? null : reader.GetString(reader.GetOrdinal("Specification")),
                                    UnitId = reader.IsDBNull(reader.GetOrdinal("UnitId")) ? null : reader.GetString(reader.GetOrdinal("UnitId")),
                                    Unit = reader.IsDBNull(reader.GetOrdinal("Unit")) ? null : reader.GetString(reader.GetOrdinal("Unit")),
                                    Price = reader.IsDBNull(reader.GetOrdinal("Price")) ? 0 : reader.GetDecimal(reader.GetOrdinal("Price")),
                                    AvailableQty = reader.IsDBNull(reader.GetOrdinal("AvailableQty")) ? 0 : reader.GetDecimal(reader.GetOrdinal("AvailableQty"))
                                };
                                products.Add(product);
                            }
                        }
                    }
                }

                if (products.Count == 0)
                {
                    return NotFound("No products found for the given user ID.");
                }

                return Ok(products);
            }
            catch (Exception ex)
            {
                // Log the exception here
                return StatusCode(500, "An error occurred while retrieving products: " + ex.Message);
            }


            return Ok();
        }



        [HttpPost("PortalReceivedPost")]
        public async Task<IActionResult> InsertPortalReceivedAsync(PortalReceivedMasterDto portaldata)
        {
            try
            {
                string systemCode = string.Empty;

                // Execute the stored procedure to generate the system code
                SqlCommand cmdSP = new SqlCommand("spMakeSystemCode", con);
                {
                    cmdSP.CommandType = CommandType.StoredProcedure;
                    cmdSP.Parameters.AddWithValue("@TableName", "PortalReceivedMaster");
                    cmdSP.Parameters.AddWithValue("@Date", DateTime.Now.ToString("yyyy-MM-dd"));
                    cmdSP.Parameters.AddWithValue("@AddNumber", 1);

                    await con.OpenAsync();
                    var tempSystem = await cmdSP.ExecuteScalarAsync();
                    systemCode = tempSystem?.ToString() ?? string.Empty;
                    await con.CloseAsync();
                }

                //int CompanyID = int.Parse(systemCode.Split('%')[0]);
                //string CompanyCode = systemCode.Split('%')[1];
                ////SP END

                //SqlCommand cmd = new SqlCommand("InsertPortalReceivedMaster", con);
                //cmd.CommandType = CommandType.Text;
                //cmd.Parameters.AddWithValue("@CompanyID", CompanyID);
                //cmd.Parameters.AddWithValue("@CompanyCode", CompanyCode);
                //cmd.Parameters.AddWithValue("@CompanyName", companyDto.CompanyName);
                //cmd.Parameters.AddWithValue("@CompanyImage", CompanyImage);
                //cmd.Parameters.AddWithValue("@CompanyFoundationDate", companyDto.CompanyFoundationDate);
                //cmd.Parameters.AddWithValue("@BusinessRegistrationNumber", companyDto.BusinessRegistrationNumber);
                //cmd.Parameters.AddWithValue("@TaxIdentificationNumber", companyDto.TaxIdentificationNumber);
                //cmd.Parameters.AddWithValue("@TradeLicense", TradeLicense);
                //cmd.Parameters.AddWithValue("@PreferredPaymentMethodID", companyDto.PreferredPaymentMethodID);
                //cmd.Parameters.AddWithValue("@BankNameID", companyDto.BankNameID ?? 0);
                //cmd.Parameters.AddWithValue("@AccountNumber", companyDto.AccountNumber ?? string.Empty);
                //cmd.Parameters.AddWithValue("@AccountHolderName", companyDto.AccountHolderName ?? string.Empty);
                //cmd.Parameters.AddWithValue("@MaxUser", 3);
                //cmd.Parameters.AddWithValue("@IsActive", -1);
                //cmd.Parameters.AddWithValue("@AddedBy", companyDto.AddedBy);
                //cmd.Parameters.AddWithValue("@DateAdded", DateTime.Now);
                //cmd.Parameters.AddWithValue("@AddedPC", companyDto.AddedPC);

                //await connection.OpenAsync();
                //await cmd.ExecuteNonQueryAsync();
                //await connection.CloseAsync();

                return Ok(new {message = "Portal data Inserted Successfully."});
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task<IActionResult> InsertPortalReceivedAsync()
        {
            try 
            {

            } 
            catch(Exception ex) 
            {
                return BadRequest(ex.Message);
            }

            return Ok(new { message = "Portal Details data Inserted Successfully." });
        }

    }
}
