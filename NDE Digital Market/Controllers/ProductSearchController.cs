using Microsoft.AspNetCore.Mvc;
using NDE_Digital_Market.Model;
using System.Data;
using System.Data.SqlClient;



namespace NDE_Digital_Market.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductSearchController : ControllerBase
    {
        private readonly string connectionDatabase;

        public ProductSearchController(IConfiguration config)
        {
            connectionDatabase = config.GetConnectionString("DigitalMarketConnection");

        }

        [HttpGet("search")]
        public IActionResult GetSearchProduct(string SearchKeyword, int Offset, int NextCount, string SortDirection)
        {

            if (Offset != 0)
            {
                Offset = 20 * (Offset - 1);
            }

            var resultList = new List<ProductSearch>();

            using (SqlConnection connection = new SqlConnection(connectionDatabase))
            {
                using (SqlCommand cmd = new SqlCommand("sp_SearchProduct", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@SortDirection", SortDirection));
                    cmd.Parameters.Add(new SqlParameter("@NextCount", NextCount));
                    cmd.Parameters.Add(new SqlParameter("@Offset", Offset));
                    cmd.Parameters.Add(new SqlParameter("@SearchKeyword", SearchKeyword));

                    connection.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resultList.Add(new ProductSearch
                            {
                                GoodsName = reader["GoodsName"].ToString(),
                                GroupCode = reader["GroupCode"].ToString(),
                                GroupName = reader["GroupName"].ToString(),
                                GoodsID = reader.GetInt32(reader.GetOrdinal("GoodsID")),
                                Specification = reader["Specification"].ToString(),
                                ApproveSalesQty = float.Parse(reader["Quantity"].ToString()),
                                Price = decimal.Parse(reader["Price"].ToString()),
                                ImagePath = reader["ImagePath"].ToString(),
                                CompanyName = reader["CompanyName"].ToString(),
                                TotalCount = int.Parse(reader["TotalCount"].ToString()),
                                SellerCode = reader["SellerCode"].ToString()
                            });
                        }
                    }
                }
            }

            return Ok(resultList);

        }

    }

    }


