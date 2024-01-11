using Microsoft.AspNetCore.Mvc;
using NDE_Digital_Market.DTOs;
using NDE_Digital_Market.SharedServices;
using System.Data;
using System.Data.SqlClient;
namespace NDE_Digital_Market.Controllers
{
    public class SellerInventoryController : Controller
    {
        private readonly string _healthCareConnection;
        public SellerInventoryController(IConfiguration configuration)
        {
            CommonServices commonServices = new CommonServices(configuration);
            _healthCareConnection = commonServices.HealthCareConnection;
        }


        [HttpGet]
        [Route("GetSellerInventoryDataBySellerId/{UserId}")]
        public async Task<IActionResult> GetSellerInventoryDataBySellerId(int UserId)
        {
            var sellerInvantoryData = new List<SellerInvantoryDataDto>();
            try
            {
                using (var connection = new SqlConnection(_healthCareConnection))
                {
                    using (var command = new SqlCommand("GetSellerInvantoryDataBySellerId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@UserId", UserId));
                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var sellerInvantory = new SellerInvantoryDataDto
                                {
                                    ProductName = reader["ProductName"].ToString(),
                                    ProductGroupName = reader["ProductGroupName"].ToString(),
                                    Specification = reader["Specification"].ToString(),
                                    Price = reader["Price"] != DBNull.Value ? Convert.ToDecimal(reader["Price"]) : 0,
                                    Unit = reader["Unit"].ToString(),
                                    TotalQty = Convert.ToInt32(reader["TotalQty"]),
                                    AvailableQty = Convert.ToInt32(reader["AvailableQty"]),
                                    SaleQty = Convert.ToInt32(reader["SaleQty"])
                                };
                                sellerInvantoryData.Add(sellerInvantory);
                            }
                        }
                    }
                }
                return Ok(sellerInvantoryData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving companies: " + ex.Message);
            }
        }

    }
}