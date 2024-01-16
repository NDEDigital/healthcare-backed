using NDE_Digital_Market.Model;
using NDE_Digital_Market.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using NDE_Digital_Market.SharedServices;

namespace NDE_Digital_Market.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoodsController : ControllerBase
    {
      
        
        private readonly string _healthCareConnection;
        public GoodsController(IConfiguration config)
        {
            CommonServices commonServices = new CommonServices(config);
            _healthCareConnection = commonServices.HealthCareConnection;
        }

        // ============ NavData ============================

        [HttpGet]
        [Route("GetNavData")]
        public async Task<ActionResult<List<NavModel>>> GetNavData()
        {
            List<NavModel> lst = new List<NavModel>();

            try
            {
                using (SqlConnection con = new SqlConnection(_healthCareConnection))
                {
                    await con.OpenAsync();
                    string query = @"SELECT * FROM ProductGroups Where IsActive = 1";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                NavModel modelObj = new NavModel
                                {
                                    ProductGroupCode = reader["ProductGroupCode"].ToString(),
                                    ProductGroupName = reader["ProductGroupName"].ToString(),
                                    ProductGroupPrefix = reader["ProductGroupPrefix"].ToString(),
                                    ProductGroupDetails = reader["ProductGroupDetails"].ToString(),
                                    ImagePath = reader["ImagePath"].ToString(),
                                    ProductGroupID = Convert.ToInt32(reader["ProductGroupID"])
                                };
                                lst.Add(modelObj);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
            return lst;
        }


[HttpGet]
[Route("GetGoodsList")]
public async Task<ActionResult<List<AllProductDto>>> GetGoodsList()
{
    List<AllProductDto> lst = new List<AllProductDto>();

    try
    {
        using (SqlConnection con = new SqlConnection(_healthCareConnection))
        {
            await con.OpenAsync();
            string query = "GetAllProductListWithAvailableQty";

            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        AllProductDto modelObj = new AllProductDto();
                        modelObj.CompanyName = reader["CompanyName"].ToString();
                        modelObj.ProductGroupName = reader["ProductGroupName"].ToString();
                        modelObj.ProductId = Convert.ToInt32(reader["ProductId"]);
                        modelObj.ProductName = reader["ProductName"].ToString();
                        modelObj.ProductGroupID = Convert.ToInt32(reader["ProductGroupID"]);
                        modelObj.Specification = reader["Specification"].ToString();
                        modelObj.UnitId = Convert.ToInt32(reader["UnitId"]);
                        modelObj.Unit = reader["Unit"].ToString();
                        modelObj.Price = reader["Price"] != DBNull.Value ? Convert.ToDecimal(reader["Price"]) : 0;
                        modelObj.DiscountAmount = reader["DiscountAmount"] != DBNull.Value ? Convert.ToDecimal(reader["DiscountAmount"]) : 0;
                        modelObj.DiscountPct = reader["DiscountPct"] != DBNull.Value ? Convert.ToDecimal(reader["DiscountPct"]) : 0;
                        modelObj.ImagePath = reader["ImagePath"].ToString();
                        modelObj.TotalPrice = reader["TotalPrice"] != DBNull.Value ? Convert.ToDecimal(reader["TotalPrice"]) : 0;
                        modelObj.SellerId = Convert.ToInt32(reader["SellerId"]);
                        modelObj.AvailableQty = Convert.ToInt32(reader["AvailableQty"]);
                        DateTime? endDate = null; 
                        if (reader["EndDate"] != DBNull.Value)
                           {    
                                    endDate = Convert.ToDateTime(reader["EndDate"]);                               
                                    if (endDate<= DateTime.Now)
                                    {
                                  
                                        modelObj.TotalPrice = modelObj.Price;
                                        modelObj.DiscountAmount = 0;
                                        modelObj.DiscountPct = 0;
                                    }
                           }
                          
                               
                              
                       lst.Add(modelObj);
                    }
                }
            }
        }
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"An error occurred: {ex.Message}");
    }
    return lst;
}




        //====================== ProductCompany =================


        [HttpGet]
        [Route("GetProductCompany/{ProductGroupCode}")]
        public async Task<IActionResult> GetProductCompany(string ProductGroupCode)
        {
            var companiesByProductGroup = new List<CompanyListDto>();
            try
            {
                using (var connection = new SqlConnection(_healthCareConnection))
                {
                    using (var command = new SqlCommand("GetCompaniesByProductGroupCode", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@ProductGroupCode", ProductGroupCode));
                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var companiesByProduct = new CompanyListDto
                                {
                                    CompanyName = reader["CompanyName"].ToString(),
                                    CompanyCode = reader["CompanyCode"].ToString(),
                                    CompanyImage = reader["CompanyImage"].ToString()
                                };
                                companiesByProductGroup.Add(companiesByProduct);
                            }
                        }
                    }
                }
                return Ok(companiesByProductGroup);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving companies: " + ex.Message);
            }

        }


        [HttpGet]
        [Route("GetProductList")]
        public async Task<IActionResult> GetProductList(string CompanyCode, string ProductGroupCode)
        {
            var goodsQuantitys = new List<CompanyProductListDto>();
            try
            {
                using (var connection = new SqlConnection(_healthCareConnection))
                {
                    using (var command = new SqlCommand("GetProductDetailsByCompanyAndGroup", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@CompanyCode", CompanyCode));
                        command.Parameters.Add(new SqlParameter("@ProductGroupCode", ProductGroupCode));
                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var goodsQuantity = new CompanyProductListDto
                                {
                                    CompanyName = reader["CompanyName"].ToString(),
                                    ProductId = Convert.ToInt32(reader["ProductId"]),
                                    ProductName = reader["ProductName"].ToString(),
                                    ProductGroupID = Convert.ToInt32(reader["ProductGroupID"]),
                                    ProductGroupName = reader["ProductGroupName"].ToString(),
                                    Specification = reader["Specification"].ToString(),
                                    UnitId = Convert.ToInt32(reader["UnitId"]),
                                    Unit = reader["Unit"].ToString(),
                                    Price = reader["Price"] != DBNull.Value ? Convert.ToDecimal(reader["Price"]) : 0,
                                    DiscountAmount = reader["DiscountAmount"] != DBNull.Value ? Convert.ToDecimal(reader["DiscountAmount"]) : 0,
                                    DiscountPct = reader["DiscountPct"] != DBNull.Value ? Convert.ToDecimal(reader["DiscountPct"]) : 0,
                                    ImagePath = reader["ImagePath"].ToString(),
                                    TotalPrice = reader["TotalPrice"] != DBNull.Value ? Convert.ToDecimal(reader["TotalPrice"]) : 0,
                                    SellerId = Convert.ToInt32(reader["SellerId"]),
                                    AvailableQty = Convert.ToInt32(reader["AvailableQty"])
                                };
                                DateTime? endDate = null;
                                if (reader["EndDate"] != DBNull.Value)
                                {
                                    endDate = Convert.ToDateTime(reader["EndDate"]);
                                    if (endDate <= DateTime.Now)
                                    {

                                        goodsQuantity.TotalPrice = goodsQuantity.Price;
                                        goodsQuantity.DiscountAmount = 0;
                                        goodsQuantity.DiscountPct = 0;
                                    }
                                }
                                goodsQuantitys.Add(goodsQuantity);
                            }
                        }
                    }
                }
               
                return Ok(goodsQuantitys);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving companies: " + ex.Message);
            }
        }
    }
}
