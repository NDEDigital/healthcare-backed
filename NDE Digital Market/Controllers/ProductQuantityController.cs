using Microsoft.AspNetCore.Mvc;
using NDE_Digital_Market.SharedServices;
using NDE_Digital_Market.Model;
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
        public ProductQuantityController(IConfiguration config)
        {
            _commonServices = new CommonServices(config);
            _healthCareConnection = config.GetConnectionString("HealthCare");
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

    }
}
