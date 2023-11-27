using Microsoft.AspNetCore.Mvc;
using NDE_Digital_Market.Model;
using System.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Authorization;

namespace NDE_Digital_Market.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class ProductReturnController : Controller
    {


 
        private readonly string _connectionDigitalMarket;
        public ProductReturnController(IConfiguration config)
        {
 
     
            _connectionDigitalMarket = config.GetConnectionString("DigitalMarketConnection");
        }



        [HttpPost, Authorize(Roles = "buyer")]
        [Route("InsertReturnedData")]
        public IActionResult InsertProductReturn([FromForm] ProductReturnModel returnData)
        {

           int returnId = 0;
            SqlConnection con = new SqlConnection(_connectionDigitalMarket);

            SqlCommand getLastReturnId = new SqlCommand("SELECT ISNULL(MAX(ReturnId), 0) FROM ProductReturn;", con);
            con.Open();

            returnId = Convert.ToInt32(getLastReturnId.ExecuteScalar()) + 1;  
            SqlCommand cmd = new SqlCommand("INSERT INTO  [ProductReturn] ([ReturnId], [GroupName],GoodsName, [GroupCode], [GoodsId], [TypeId], [Remarks],[Price],[DetailsId],[SellerCode],[ApplyDate],[OrderNo], [DeliveryDate]) VALUES (@ReturnId, @GroupName,@GoodsName, @GroupCode, @GoodsId, @TypeId, @Remarks , @Price, @DetailsId, @SellerCode,GETDATE(),@OrderNo,@DeliveryDate);", con);
          
            using (cmd)
            {
                cmd.Parameters.AddWithValue("@returnId", returnId);
                cmd.Parameters.AddWithValue("@GroupName", returnData.GroupName);
                cmd.Parameters.AddWithValue("@GroupCode", returnData.GroupCode);
                cmd.Parameters.AddWithValue("@GoodsId", returnData.GoodsId);
                cmd.Parameters.AddWithValue("@TypeId", returnData.TypeId);
                cmd.Parameters.AddWithValue("@Remarks", string.IsNullOrEmpty(returnData.Remarks) ? (object)DBNull.Value : returnData.Remarks);
                cmd.Parameters.AddWithValue("@Price", returnData.Price);
                cmd.Parameters.AddWithValue("@DetailsId", returnData.DetailsId);
                cmd.Parameters.AddWithValue("@SellerCode", returnData.SellerCode);
                cmd.Parameters.AddWithValue("@OrderNo", returnData.OrderNo);
                cmd.Parameters.AddWithValue("@GoodsName", returnData.GoodsName ?? " ");
                cmd.Parameters.AddWithValue("@DeliveryDate", returnData.DeliveryDate);


                cmd.ExecuteNonQuery();
            }



            con.Close();



            SqlCommand command = new SqlCommand("UPDATE OrderDetails SET Status = 'to Return' WHERE OrderDetailId = " + returnData.DetailsId + "", con);
 
            con.Open();
            command.ExecuteNonQuery();
            con.Close();
            return Ok();
            
        }

        [HttpGet, Authorize(Roles = "buyer")]
        [Route("GetReturnType")]
        public IActionResult GetReturnTypeData()
        {
            List<ProductReturnModel> returnTypeList = new List<ProductReturnModel>();
 
            string sqlSelect = "SELECT [TypeId], [ReturnType] FROM  [ReturnType]";

    
            using (SqlConnection connection = new SqlConnection(_connectionDigitalMarket))
            {
         
                using (SqlCommand cmd = new SqlCommand(sqlSelect, connection))
                {
                    try
                    {             
                        connection.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            ProductReturnModel returnType = new ProductReturnModel
                            {
                                TypeId = (int)reader["TypeId"],
                                ReturnType = reader["ReturnType"].ToString()
                            };

                            returnTypeList.Add(returnType);
                        }
                        reader.Close();

                        return Ok(returnTypeList); 
                    }
                    catch (Exception ex)
                    {
                        return BadRequest($"Error: {ex.Message}");
                    }
                }
            }
        }
    }



}




 
