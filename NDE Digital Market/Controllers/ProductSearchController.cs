using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NDE_Digital_Market.DTOs;
using NDE_Digital_Market.Model;
using System.Data;
using System.Data.SqlClient;
using NDE_Digital_Market.SharedServices;


namespace NDE_Digital_Market.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductSearchController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string connectionDatabase;
        private readonly string _healthCareConnection;

        public ProductSearchController(IConfiguration config)
        {
            _configuration = config;
            CommonServices commonServices = new CommonServices(config);
            connectionDatabase = config.GetConnectionString("DigitalMarketConnection");
            _healthCareConnection = commonServices.HealthCareConnection;

        }

        [HttpGet("GetSearchedProduct")]
        public IActionResult GetSearchedProduct(string productName, string sortDirection, int nextCount, int offset)
        {
            if (offset != 0)
            {
                offset = 20 * (offset - 1);
            }

            var resultList = new List<ProductSearchDto>();

            using (SqlConnection connection = new SqlConnection(_healthCareConnection))
            {
                using (SqlCommand cmd = new SqlCommand("ProductSearch", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@ProductName", productName));
                    cmd.Parameters.Add(new SqlParameter("@SortDirection", sortDirection));
                    cmd.Parameters.Add(new SqlParameter("@NextCount", nextCount));
                    cmd.Parameters.Add(new SqlParameter("@Offset", offset));

                    connection.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                                var productSearchDto = new ProductSearchDto();

                                productSearchDto.CompanyName = reader["CompanyName"] is DBNull ? null : (string)reader["CompanyName"];
                                productSearchDto.ProductId = reader["ProductId"] is DBNull ? (int?)null : (int)reader["ProductId"];
                                productSearchDto.ProductName = reader["ProductName"] is DBNull ? null : (string)reader["ProductName"];
                                productSearchDto.ProductGroupID = reader["ProductGroupID"] is DBNull ? (int?)null : (int)reader["ProductGroupID"];
                                productSearchDto.ProductGroupName = reader["ProductGroupName"] is DBNull ? null : (string)reader["ProductGroupName"];
                                productSearchDto.Specification = reader["Specification"] is DBNull ? null : (string)reader["Specification"];
                                productSearchDto.UnitId = reader["UnitId"] is DBNull ? (int?)null : (int)reader["UnitId"];
                                productSearchDto.Unit = reader["Unit"] is DBNull ? null : (string)reader["Unit"];
                                productSearchDto.Price = reader["Price"] is DBNull ? (decimal?)null : (decimal)reader["Price"];
                                productSearchDto.DiscountAmount = reader["DiscountAmount"] is DBNull ? (decimal?)null : (decimal)reader["DiscountAmount"];
                                productSearchDto.DiscountPct = reader["DiscountPct"] is DBNull ? (decimal?)null : (decimal)reader["DiscountPct"];
                                productSearchDto.ImagePath = reader["ImagePath"] is DBNull ? null : (string)reader["ImagePath"];
                                productSearchDto.TotalPrice = reader["TotalPrice"] is DBNull ? (decimal?)null : (decimal)reader["TotalPrice"];
                                productSearchDto.SellerId = reader["SellerId"] is DBNull ? (int?)null : (int)reader["SellerId"];
                                productSearchDto.AvailableQty = reader["AvailableQty"] is DBNull ? (decimal?)null : (decimal)reader["AvailableQty"];
                                productSearchDto.TotalCount = reader["TotalCount"] is DBNull ? (int?)null : (int)reader["TotalCount"];

                                resultList.Add(productSearchDto);

                        }
                    }
                }
            }

            return Ok(resultList);
        }


    }

}


