using Microsoft.AspNetCore.Mvc;
using NDE_Digital_Market.Model;
using System.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using NDE_Digital_Market.DTOs;

namespace NDE_Digital_Market.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class ProductReturnController : Controller
    {


 
        private readonly string _connectionHealthCare;
        public ProductReturnController(IConfiguration config)
        {
            _connectionHealthCare = config.GetConnectionString("HealthCare");
        }



        //[HttpPost, Authorize(Roles = "buyer")]
        [HttpPost]
        [Route("InsertReturnedData")]
        public async Task<IActionResult> InsertProductReturn([FromForm] ProductReturnDto returnData)
        {
            SqlTransaction transaction = null;
            SqlConnection con = new SqlConnection(_connectionHealthCare);

            try
            {
                await con.OpenAsync();
                transaction = con.BeginTransaction();
                string systemCode = string.Empty;

                // Execute the stored procedure to generate the system code
                SqlCommand cmdSP = new SqlCommand("spMakeSystemCode", con, transaction);
                {
                    cmdSP.CommandType = CommandType.StoredProcedure;
                    cmdSP.Parameters.AddWithValue("@TableName", "ProductReturn");
                    cmdSP.Parameters.AddWithValue("@Date", DateTime.Now.ToString("yyyy-MM-dd"));
                    cmdSP.Parameters.AddWithValue("@AddNumber", 1);
                    var tempSystem = await cmdSP.ExecuteScalarAsync();

                    systemCode = tempSystem?.ToString() ?? string.Empty;
                }
                int ProductReturnId = int.Parse(systemCode.Split('%')[0]);
                string ProductReturnCode = systemCode.Split('%')[1];
                // SP END

                string query = @"INSERT INTO ProductReturn(ProductReturnId,ProductReturnCode,ProductGroupId,ProductId,OrderNo,Price,
                                    OrderDetailsId,SellerId,ApplyDate,DeliveryDate,Remarks,AddedDate,AddedBy,AddedPc)
                                VALUES(@ProductReturnId,@ProductReturnCode,@ProductGroupId,@ProductId,@OrderNo,@Price,
                                    @OrderDetailsId,@SellerId,@ApplyDate,@DeliveryDate,@Remarks,@AddedDate,@AddedBy,@AddedPc);";


                SqlCommand cmd = new SqlCommand(query, con, transaction);
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@ProductReturnId", ProductReturnId);
                cmd.Parameters.AddWithValue("@ProductReturnCode", ProductReturnCode);
                cmd.Parameters.AddWithValue("@ProductGroupId", returnData.ProductGroupId);
                cmd.Parameters.AddWithValue("@ProductId", returnData.ProductId);
                cmd.Parameters.AddWithValue("@OrderNo", returnData.OrderNo);
                cmd.Parameters.AddWithValue("@Price", returnData.Price);
                cmd.Parameters.AddWithValue("@OrderDetailsId", returnData.OrderDetailsId);
                cmd.Parameters.AddWithValue("@SellerId", returnData.SellerId);
                cmd.Parameters.AddWithValue("@ApplyDate", returnData.ApplyDate);
                cmd.Parameters.AddWithValue("@DeliveryDate", returnData.DeliveryDate);
                cmd.Parameters.AddWithValue("@Remarks", returnData.Remarks);

                cmd.Parameters.AddWithValue("@AddedDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@AddedBy", returnData.AddedBy);
                cmd.Parameters.AddWithValue("@AddedPc", returnData.AddedPc);

                int a = await cmd.ExecuteNonQueryAsync();
                if (a > 0)
                {
                    SqlCommand command = new SqlCommand("UPDATE OrderDetails SET Status = 'to Return' WHERE OrderDetailId = " + returnData.OrderDetailsId + "", con, transaction);

                    int updateResult = await command.ExecuteNonQueryAsync();

                    if (updateResult <= 0)
                    {

                        transaction.Rollback();
                        return BadRequest(new { message = "Order Details status isn't change or not found." });
                    }
                }
                else
                {
                    return BadRequest(new { message = "ProductReturn data isn't Inserted Successfully." });
                }

                // If everything is fine, commit the transaction
                transaction.Commit();
                return Ok(new { message = "ProductReturn data Inserted Successfully." });

            }
            catch(Exception ex)
            {
                // If there is any error, rollback the transaction
                if (transaction != null)
                {
                    transaction.Rollback();
                }
                return BadRequest(new { message = ex.Message });
            }
            finally
            {
                // Finally block to ensure the connection is always closed
                if (con.State == ConnectionState.Open)
                {
                    await con.CloseAsync();
                }
            }

        }







        //[HttpGet, Authorize(Roles = "buyer")]
        //[Route("GetReturnType")]
        //public IActionResult GetReturnTypeData()
        //{
        //    List<ProductReturnModel> returnTypeList = new List<ProductReturnModel>();
 
        //    string sqlSelect = "SELECT [TypeId], [ReturnType] FROM  [ReturnType]";

    
        //    using (SqlConnection connection = new SqlConnection(_connectionDigitalMarket))
        //    {
         
        //        using (SqlCommand cmd = new SqlCommand(sqlSelect, connection))
        //        {
        //            try
        //            {             
        //                connection.Open();
        //                SqlDataReader reader = cmd.ExecuteReader();
        //                while (reader.Read())
        //                {
        //                    ProductReturnModel returnType = new ProductReturnModel
        //                    {
        //                        TypeId = (int)reader["TypeId"],
        //                        ReturnType = reader["ReturnType"].ToString()
        //                    };

        //                    returnTypeList.Add(returnType);
        //                }
        //                reader.Close();

        //                return Ok(returnTypeList); 
        //            }
        //            catch (Exception ex)
        //            {
        //                return BadRequest($"Error: {ex.Message}");
        //            }
        //        }
        //    }
        //}


    }



}




 
