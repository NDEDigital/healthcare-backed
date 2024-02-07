using Microsoft.AspNetCore.Mvc;
using NDE_Digital_Market.Model;
using NDE_Digital_Market.SharedServices;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using NDE_Digital_Market.Model.MaterialStock;
using static Org.BouncyCastle.Math.EC.ECCurve;
using NDE_Digital_Market.DTOs;

namespace NDE_Digital_Market.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly SqlConnection con;
        private readonly string _healthCareConnection;
        //private readonly IWebHostEnvironment _hostingEnvironment;
        //public ProductsController(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        private CommonServices _commonServices;
        public ProductsController(IConfiguration configuration)
        {
            _configuration = configuration;
            _commonServices = new CommonServices(configuration);
            //_hostingEnvironment = hostingEnvironment;
            con = new SqlConnection(_commonServices.HealthCareConnection);
            _healthCareConnection = _commonServices.HealthCareConnection;

            //string rootPath = _hostingEnvironment.ContentRootPath;
            //Console.WriteLine(rootPath);
        }


        //===================================== Create User ================================


        [HttpPut, Authorize(Roles = "seller")]
        [Route("UpdateProduct")]
        public IActionResult UpdateProduct([FromForm] GoodsQuantityModel product)
        {
            string decryptedSupplierCode = CommonServices.DecryptPassword(product.SellerCode);
            product.UpdatedBy = decryptedSupplierCode;
            product.UpdatedDate = DateTime.Now;
            SqlCommand cmd = new SqlCommand("INSERT INTO EditedProductList (GoodsId, GoodsName, Specification, GroupCode, GroupName,Quantity,Price, QuantityUnit, UpdatedDate, UpdatedBy, UpdatedPc, Status, SellerCode)VALUES( @GoodsId, @GoodsName, @Specification, @GroupCode, @GroupName,@Price, @Quantity, @QuantityUnit, @UpdatedDate, @UpdatedBy, @UpdatedPc, @Status,  @SellerCode);" +
                "UPDATE ProductList SET Status=@Status WHERE  SellerCode = @SellerCode AND GoodsId = @GoodsId", con);
            cmd.CommandType = CommandType.Text;


            cmd.Parameters.AddWithValue("@GoodsId", product.GoodsId);
            cmd.Parameters.AddWithValue("@GoodsName", product.GoodsName);
            cmd.Parameters.AddWithValue("@Specification", product.Specification);
            cmd.Parameters.AddWithValue("@GroupCode", product.GroupCode);
            cmd.Parameters.AddWithValue("@GroupName", product.GroupName);
            cmd.Parameters.AddWithValue("@Quantity", product.Quantity);
            cmd.Parameters.AddWithValue("@Price", product.Price);
            cmd.Parameters.AddWithValue("@QuantityUnit", product.QuantityUnit);
            cmd.Parameters.AddWithValue("@UpdatedDate", product.UpdatedDate);
            cmd.Parameters.AddWithValue("@UpdatedBy", product.UpdatedBy);
            cmd.Parameters.AddWithValue("@UpdatedPc", product.UpdatedPc);
            cmd.Parameters.AddWithValue("@Status", product.Status);
            cmd.Parameters.AddWithValue("@SellerCode", decryptedSupplierCode);

        
            try
            {
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
                return Ok();
            }
            catch (SqlException ex)
            {
                return BadRequest("SQL Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
        }

       
        
        // ======================= GET Dashboard Contents ================== 

        [HttpGet ]
        [Route("GetDashboardContents")]

        public IActionResult GetDashboardContents(string sellerCode, String? status = null, String? productName = null, String? companyName = null, DateTime? addedDate = null)
        {
            try
            {
                Console.WriteLine(sellerCode, "sellerCode");
                string decryptedSupplierCode = CommonServices.DecryptPassword(sellerCode);

                bool isAdmin = false;
                int newCount = 0, editedCount = 0, approvedCount = 0, rejectedCount = 0;
                string query = "SELECT PhoneNumber FROM UserRegistration WHERE UserCode = @UserCode;";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UserCode", decryptedSupplierCode);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    string phoneNumber = reader["PhoneNumber"].ToString();

                    if (phoneNumber == "admin")
                    {
                        isAdmin = true;
                    }
                }
                con.Close();
                List<GoodsQuantityModel> Products = new List<GoodsQuantityModel>();

                if (isAdmin && status != null)
                {

                    Console.WriteLine(isAdmin);
                    Console.WriteLine("isAdmin");

                    string queryForAdmin = "sp_ProductListWithCompanyName";

                    SqlCommand cmdForAdmin = new SqlCommand(queryForAdmin, con);
                    cmdForAdmin.CommandType = CommandType.StoredProcedure;
                    cmdForAdmin.Parameters.AddWithValue("@Status", status);
                    if (productName != null) { cmdForAdmin.Parameters.AddWithValue("@productName", productName); }
                    if (companyName != null) { cmdForAdmin.Parameters.AddWithValue("@CompanyName", companyName); }
                    if (addedDate != null) { cmdForAdmin.Parameters.AddWithValue("@AddedDate", addedDate); }
                    SqlDataAdapter adapter = new SqlDataAdapter(cmdForAdmin);
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
                    DataTable dt = ds.Tables[0];
                    DataTable dt1 = ds.Tables[1];

                    con.Close();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        newCount = Convert.ToInt32(dt.Rows[i]["NewCount"]);
                        editedCount = Convert.ToInt32(dt.Rows[i]["EditedCount"]);
                        approvedCount = Convert.ToInt32(dt.Rows[i]["ApprovedCount"]);
                        rejectedCount = Convert.ToInt32(dt.Rows[i]["RejectedCount"]);
                    }

                    for (int i = 0; i < dt1.Rows.Count; i++)
                    {
                        GoodsQuantityModel modelObj = new GoodsQuantityModel();
                        modelObj.CompanyName = dt1.Rows[i]["CompanyName"].ToString();
                        modelObj.GroupCode = dt1.Rows[i]["GroupCode"].ToString();
                        modelObj.GoodsId = dt1.Rows[i]["GoodsID"].ToString();
                        modelObj.GroupName = dt1.Rows[i]["GroupName"].ToString();
                        modelObj.GoodsName = dt1.Rows[i]["GoodsName"].ToString();
                        modelObj.Specification = dt1.Rows[i]["Specification"].ToString();
                        modelObj.ApproveSalesQty = float.Parse(dt1.Rows[i]["Quantity"].ToString());
                        modelObj.SellerCode = dt1.Rows[i]["SellerCode"].ToString();
                        modelObj.Price = float.Parse(dt1.Rows[i]["Price"].ToString());
                        modelObj.QuantityUnit = dt1.Rows[i]["QuantityUnit"].ToString();
                        modelObj.ImagePath = dt1.Rows[i]["ImagePath"].ToString();
                        modelObj.AddedDate = dt1.Rows[i]["AddedDate"] != DBNull.Value ? Convert.ToDateTime(dt1.Rows[i]["AddedDate"]) : (DateTime?)null;
                        Products.Add(modelObj);
                    }
                }
                else
                {
                    isAdmin = false;
                }
                return Ok(new { message = "content get successfully", Products, isAdmin, newCount, editedCount, approvedCount, rejectedCount });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return BadRequest(new { message = "An error occurred while processing the request." });
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }


        // ======================= GET Product ==================

        //[HttpGet]
        //[Route("GetProduct")]
        //public List<GoodsQuantityModel>GetSellerProduct(string sellerCode)
        //{
        //    Console.WriteLine(sellerCode, "sellerCode");
        //    string decryptedSupplierCode = CommonServices.DecryptPassword(sellerCode);

        //    string query = @"SELECT 
        //                    ProductList.GoodsId, 
        //                    ProductList.GoodsName, 
        //                    ProductList.GroupCode,
        //                    ProductList.GroupName,
        //                    ProductList.Specification,
        //                    ProductList.Price,
        //                    ProductList.SellerCode,
        //                    ProductList.ImagePath,
        //                    ISNULL(MaterialStockQty.PresentQty, 0) AS Quantity,
        //                    ProductList.QuantityUnit,  
        //                 UserRegistration.CompanyName

        //                 FROM ProductList
        //                LEFT JOIN
        //                UserRegistration
        //                ON
        //                ProductList.SellerCode = UserRegistration.UserCode
        //                LEFT JOIN
        //                                        MaterialStockQty
        //                                        ON
        //                                           MaterialStockQty.GroupCode = ProductList.GroupCode AND MaterialStockQty.GoodsId = ProductList.GoodsId
        //                WHERE ProductList.SellerCode = @DecryptedSupplierCode  ORDER BY ProductList.UpdatedDate DESC; ";
        //    SqlCommand cmd = new SqlCommand(query, con);
        //    cmd.Parameters.AddWithValue("@DecryptedSupplierCode", decryptedSupplierCode);

        //    con.Open();
        //    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
        //    DataTable dt = new DataTable();

        //    adapter.Fill(dt);

        //    con.Close();

        //    List<GoodsQuantityModel> sellerProducts = new List<GoodsQuantityModel>();

        //    for (int i = 0; i < dt.Rows.Count; i++)
        //    {
        //        GoodsQuantityModel modelObj = new GoodsQuantityModel();


        //        modelObj.CompanyName = dt.Rows[i]["CompanyName"].ToString();
        //        modelObj.GroupCode = dt.Rows[i]["GroupCode"].ToString();
        //        modelObj.GoodsId = dt.Rows[i]["GoodsID"].ToString();
        //        modelObj.GroupName = dt.Rows[i]["GroupName"].ToString();
        //        modelObj.GoodsName = dt.Rows[i]["GoodsName"].ToString();
        //        modelObj.Specification = dt.Rows[i]["Specification"].ToString();
        //        modelObj.ApproveSalesQty = float.Parse(dt.Rows[i]["Quantity"].ToString());
        //        modelObj.SellerCode = dt.Rows[i]["SellerCode"].ToString();
        //        modelObj.Price = float.Parse(dt.Rows[i]["Price"].ToString());
        //        modelObj.QuantityUnit = dt.Rows[i]["QuantityUnit"].ToString();
        //        modelObj.ImagePath = dt.Rows[i]["ImagePath"].ToString();

        //        sellerProducts.Add(modelObj);

        //    }
        //    return sellerProducts;

        //}



        // ====================== new GET Product ==========================



        [HttpGet("GetSellerProductForAdminApproval")]
        public async Task<IActionResult> GetSellerProductForAdminApproval(string status)
        {
            //string DecryptId = CommonServices.DecryptPassword(companyCode);
            var products = new List<ProductStatusDto>();

            try
            {
                using (var connection = new SqlConnection(_healthCareConnection))
                {
                    using (var command = new SqlCommand("SellerProductStatus", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@Status", status));

                        await connection.OpenAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {

                                var product = new ProductStatusDto
                                {
                                    ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                    ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                                    FullName = reader.GetString(reader.GetOrdinal("FullName")),
                                    Price = reader.IsDBNull(reader.GetOrdinal("Price")) ? (decimal?)null : (decimal?)reader.GetDecimal(reader.GetOrdinal("Price")),
                                    DiscountAmount = reader.IsDBNull(reader.GetOrdinal("DiscountAmount")) ? (decimal?)null : (decimal?)reader.GetDecimal(reader.GetOrdinal("DiscountAmount")),
                                    DiscountPct = reader.IsDBNull(reader.GetOrdinal("DiscountPct")) ? (decimal?)null : (decimal?)reader.GetDecimal(reader.GetOrdinal("DiscountPct")),
                                    EffectivateDate = reader.IsDBNull(reader.GetOrdinal("EffectivateDate")) ? (DateTime?)null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("EffectivateDate")),
                                    EndDate = reader.IsDBNull(reader.GetOrdinal("EndDate")) ? (DateTime?)null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("EndDate")),
                                    ImagePath = reader.IsDBNull(reader.GetOrdinal("ImagePath")) ? null : reader.GetString(reader.GetOrdinal("ImagePath")),
                                    TotalPrice = reader.IsDBNull(reader.GetOrdinal("TotalPrice")) ? (decimal?)null : (decimal?)reader.GetDecimal(reader.GetOrdinal("TotalPrice")),
                                    CompanyCode = reader.IsDBNull(reader.GetOrdinal("CompanyCode")) ? null : reader.GetString(reader.GetOrdinal("CompanyCode")),
                                    CompanyName = reader.IsDBNull(reader.GetOrdinal("CompanyName")) ? null : reader.GetString(reader.GetOrdinal("CompanyName")),
                                    Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? null : reader.GetString(reader.GetOrdinal("Status")),
                                    PreviousPrice = reader.IsDBNull(reader.GetOrdinal("PPrice")) ? (decimal?)null : (decimal?)reader.GetDecimal(reader.GetOrdinal("PPrice")),
                                    PreviousDiscountAmount = reader.IsDBNull(reader.GetOrdinal("PDiscountAmount")) ? (decimal?)null : (decimal?)reader.GetDecimal(reader.GetOrdinal("PDiscountAmount")),
                                    PreviousDiscountPct = reader.IsDBNull(reader.GetOrdinal("PDiscountPct")) ? (decimal?)null : (decimal?)reader.GetDecimal(reader.GetOrdinal("PDiscountPct")),
                                    PreviousTotalPrice = reader.IsDBNull(reader.GetOrdinal("PTotalPrice")) ? (decimal?)null : (decimal?)reader.GetDecimal(reader.GetOrdinal("PTotalPrice"))

                                };
                                products.Add(product);
                            }
                        }
                    }
                }

                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving products: " + ex.Message);
            }
        }

        // ======================= DELETE Product ==================

        [HttpDelete, Authorize(Roles = "seller")]
        [Route("DeleteProduct")]
        public IActionResult DeleteProcuct(string sellerCode, int ProductId)
        {
            try
            {
                string decryptedSupplierCode = CommonServices.DecryptPassword(sellerCode);

                SqlCommand cmd = new SqlCommand("DELETE FROM ProductList WHERE  GoodsId = @GoodsId AND SellerCode = @SellerCode", con);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@GoodsId", ProductId);
                cmd.Parameters.AddWithValue("@SellerCode", decryptedSupplierCode);
                con.Open();
                cmd.ExecuteNonQuery();
                return Ok(new { message = "Product DELETED successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return BadRequest(new { message = "An error occurred while processing the request." });
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }


        //================== SellerProductPriceAndOffer status Update by Tushar ==================
        [HttpPut("SellerProductStatusUpdate")]
        public async Task<IActionResult> UpdateSellerProductStatusAsync(List<ProductStatusDto> productStatusList)
        {
            try
            {
                string query = @"UPDATE SellerProductPriceAndOffer SET Status = @Status, UpdatedDate = @UpdatedDate WHERE ProductId = @ProductId AND UserId = @UserId AND CompanyCode = @CompanyCode ";

                using (var connection = new SqlConnection(_healthCareConnection))
                {
                    await connection.OpenAsync();

                    foreach (var productStatus in productStatusList)
                    {
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Status", productStatus.Status);
                            command.Parameters.AddWithValue("@UserId", productStatus.UserId);
                            command.Parameters.AddWithValue("@CompanyCode", productStatus.CompanyCode);
                            command.Parameters.AddWithValue("@ProductId", productStatus.ProductId);
                            command.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);

                            // Execute the command
                            await command.ExecuteNonQueryAsync();
                        }
                    }

                    await connection.CloseAsync();
                }

                return Ok(new { message = "SellerProduct status Changed successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //[HttpPut]
        //[Route("UpdateProductStatus")]
        //public IActionResult UpdateProductStatus([FromForm] UpdateProductStatusModel Obj)
        //{

        //    string decryptedSupplierCode = CommonServices.DecryptPassword(Obj.userCode);
        //    bool cancelEdited = false;
        //    List<EditedUserInfoModel> users = new List<EditedUserInfoModel>();
        //    string UpdatedBy = decryptedSupplierCode;
        //    DateTime UpdateDate = DateTime.Now;
        //    //Console.WriteLine(Obj.productIDs);
        //    int statusBit = 1;
        //    if (Obj.status == "approved")
        //    {
        //        statusBit = 2;
        //    }
        //    if (Obj.status == "rejected") 
        //    {
        //        statusBit = 3; 
        //    }

        //    if (Obj.statusBefore == "edited")
        //    {
        //        StringBuilder queryEdited = new StringBuilder();
        //        if (Obj.status == "rejected")
        //        {
        //            cancelEdited = true;
        //            queryEdited = queryEdited.Append($"UPDATE ProductList  SET Status = 'approved', UpdatedBy = '{UpdatedBy}', UpdatedDate = '{UpdateDate}',UpdatedPc= '{Obj.updatedPC}'  WHERE GoodsId IN ({Obj.productIDs})");
        //            //queryEdited = queryEdited.Append($"SELECT SupplierCode,email,full_name FROM ProductList LEFT JOIN UserRegistration ON SupplierCode=UserCode WHERE ProductID IN ({Obj.productIDs})");
        //        }
        //        else
        //        {
        //            // Split the product IDs into an array

        //            string[] productIDsArray = Obj.productIDs.Split(',');

        //            // Loop through each product ID
        //            foreach (string productId in productIDsArray)
        //            {

        //                queryEdited.Append($"UPDATE ProductList SET GoodsName = EPL.GoodsName, Specification = EPL.Specification, " +
        //                $"GroupCode = EPL.GroupCode,GroupName = EPL.GroupName," +
        //                $"Price = EPL.Price,Quantity = EPL.Quantity,QuantityUnit" +
        //                $" = EPL.QuantityUnit, ");

        //                queryEdited.Append($"Status = '{Obj.status}',  UpdatedBy = '{UpdatedBy}', UpdatedDate = '{UpdateDate}',updatedPc= '{Obj.updatedPC}'");
        //                queryEdited.Append($"FROM (SELECT * FROM EditedProductList WHERE GoodsId = {productId}) AS EPL ");
        //                queryEdited.Append($"WHERE ProductList.Goodsid = {productId} AND ProductList.SellerCode = EPL.SellerCode;");


        //            }
        //        }

        //        StringBuilder deleteQuery = new StringBuilder();
        //        deleteQuery.Append($"DELETE FROM EditedProductList WHERE GoodsId IN ({Obj.productIDs})");
        //        string combinedQuery = queryEdited.ToString() + deleteQuery.ToString();
        //        SqlCommand cmdEdite = new SqlCommand(combinedQuery, con);
        //        con.Open();
        //        cmdEdite.CommandType = CommandType.Text;
        //        cmdEdite.ExecuteNonQuery();

        //        string selectQuery = $"SELECT SellerCode, Email, FullName,GoodsName FROM ProductList LEFT JOIN UserRegistration ON SellerCode=UserCode WHERE GoodsId IN ({Obj.productIDs})";
        //        using (SqlCommand selectCmd = new SqlCommand(selectQuery, con))
        //        {
        //            selectCmd.CommandType = CommandType.Text;

        //            using (SqlDataReader reader = selectCmd.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    EditedUserInfoModel user = new EditedUserInfoModel();
        //                    user.SupplierCode = reader["SellerCode"].ToString();
        //                    user.Email = reader["Email"].ToString();
        //                    user.FullName = reader["FullName"].ToString();
        //                    user.ProductName = reader["GoodsName"].ToString();
        //                    users.Add(user);
        //                }
        //                // Now you can use the 'users' list with the retrieved data.
        //            }

        //        }
        //        con.Close();



        //    }
        //    else
        //    {
        //        string query = $"UPDATE ProductList  SET Status = '{Obj.status}', UpdatedBy = '{UpdatedBy}', UpdatedDate = '{UpdateDate}',UpdatedPc= '{Obj.updatedPC}'  WHERE GoodsId IN ({Obj.productIDs})";
        //        SqlCommand cmd = new SqlCommand(query, con);

        //        con.Open();
        //        cmd.CommandType = CommandType.Text;
        //        cmd.ExecuteNonQuery();
        //        con.Close();
        //    }

        //    return Ok(new { message = "Product status updated successfully", cancelEdited, users });
        //}









        public class EditedUserInfoModel
        {

            public string? FullName { get; set; }
            public string? SupplierCode { get; set; }
            public string? Email { get; set; }
            public string? ProductName { get; set; }
        }


        [HttpGet ]
        [Route("comapreEditedProduct")]
        public IActionResult comapreEditedProduct(int productId)
        {
            try
            {
                GoodsQuantityModel oldData = new GoodsQuantityModel();
                GoodsQuantityModel newData = new GoodsQuantityModel();
                string query = "SELECT * FROM ProductList WHERE GoodsId=@ProductId; SELECT * FROM EditedProductList WHERE GoodsId=@ProductId;";
                con.Open();
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@ProductId", productId);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                DataTable dt0 = ds.Tables[0];
                DataTable dt1 = ds.Tables[1];
                con.Close();
                for (int i = 0; i < dt0.Rows.Count; i++)
                {
                    oldData.GoodsId = dt0.Rows[i]["GoodsId"].ToString();
                    oldData.Status = dt0.Rows[i]["Status"].ToString();
                    oldData.GoodsName = dt0.Rows[i]["GoodsName"].ToString();
                    oldData.Specification = dt0.Rows[i]["Specification"].ToString();
                    oldData.GroupCode = dt0.Rows[i]["GroupCode"].ToString();
                    oldData.GroupName = dt0.Rows[i]["GroupName"].ToString();
                    oldData.Price = Convert.ToSingle(dt0.Rows[i]["Price"]);
                    oldData.ImagePath = dt0.Rows[i]["ImagePath"].ToString();
                    oldData.SellerCode = dt0.Rows[i]["SellerCode"].ToString();
                    oldData.Quantity = Convert.ToInt32(dt0.Rows[i]["Quantity"].ToString());
                    oldData.QuantityUnit = dt0.Rows[i]["QuantityUnit"].ToString();
                }
                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    newData.GoodsId = dt1.Rows[i]["GoodsId"].ToString();
                    newData.Status = dt1.Rows[i]["Status"].ToString();
                    newData.GoodsName = dt1.Rows[i]["GoodsName"].ToString();
                    newData.Specification = dt1.Rows[i]["Specification"].ToString();
                    newData.GroupCode = dt1.Rows[i]["GroupCode"].ToString();
                    newData.GroupName = dt1.Rows[i]["GroupName"].ToString();
                    newData.Price = Convert.ToSingle(dt1.Rows[i]["Price"]);
                    newData.ImagePath = dt1.Rows[i]["ImagePath"].ToString();
                    newData.SellerCode = dt1.Rows[i]["SellerCode"].ToString();
                    newData.Quantity = Convert.ToInt32(dt1.Rows[i]["Quantity"].ToString());
                    newData.QuantityUnit = dt1.Rows[i]["QuantityUnit"].ToString();
                }
                return Ok(new { message = "GET Products data successful", oldData, newData });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return BadRequest(new { message = "An error occurred while processing the request." });
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
