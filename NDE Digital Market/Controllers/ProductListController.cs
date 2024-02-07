using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NDE_Digital_Market.DTOs;
using NDE_Digital_Market.Model;
using NDE_Digital_Market.SharedServices;
using System.Data;
using System.Data.SqlClient;

namespace NDE_Digital_Market.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductListController : ControllerBase
    {

        private readonly string foldername;
        private readonly IConfiguration _configuration;
        private readonly SqlConnection con;
        private readonly string filename = "Productfile";
        public ProductListController(IConfiguration configuration)
        {
            _configuration = configuration;
            CommonServices commonServices = new CommonServices(_configuration);
            con = new SqlConnection(commonServices.HealthCareConnection);       
            foldername = commonServices.FilesPath + "Productfiles";
        }

        private async Task<Boolean> ProductNameCheck(string ProductName,string Specification)
        {

            string query = @"SELECT COUNT(*) FROM ProductList WHERE ProductName = @ProductName AND Specification = @Specification;";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@ProductName", ProductName);
            cmd.Parameters.AddWithValue("@Specification", Specification);
            await con.OpenAsync();
            int count = (int)await cmd.ExecuteScalarAsync();
            await con.CloseAsync();
            Boolean check = false;
            if (count > 0)
            {
                check = true;
            }
            return check;
        }


        private async Task<Boolean> ProductListExist(int? ProductId)
        {
            if (ProductId.HasValue)
            {
                string query = @"SELECT COUNT(*) FROM ProductList WHERE ProductId = @ProductId";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@ProductId", ProductId.Value);
                await con.OpenAsync();
                int count = (int)await cmd.ExecuteScalarAsync();
                await con.CloseAsync();
                Boolean check = false;
                if (count > 0)
                {
                    check = true;
                }
                return check;
            }
            else
            {
                return false;
            }
        }



        [HttpPost("CreateProductList")]
        public async Task<IActionResult> CreateProductGroupsAsync([FromForm] ProductListDto productListDto)
        {
            try
            {
                Boolean check = await ProductNameCheck(productListDto.ProductName, productListDto.Specification);
                if (check)
                {
                    return BadRequest(new { message = "ProductName and Specification is same!" });
                }
                else
                {
                    string systemCode = string.Empty;

                    SqlCommand cmdSP = new SqlCommand("spMakeSystemCode", con);
                    {
                        cmdSP.CommandType = CommandType.StoredProcedure;
                        cmdSP.Parameters.AddWithValue("@TableName", "ProductList");
                        cmdSP.Parameters.AddWithValue("@Date", DateTime.Now.ToString("yyyy-MM-dd"));
                        cmdSP.Parameters.AddWithValue("@AddNumber", 1);

                        await con.OpenAsync();
                        var tempSystem = await cmdSP.ExecuteScalarAsync();
                        systemCode = tempSystem?.ToString() ?? string.Empty;
                        await con.CloseAsync();
                    }
                    string ImagePath = CommonServices.UploadFiles(foldername, filename, productListDto.ImageFile);
                    if(ImagePath == null)
                    {
                        return BadRequest(new { message = "Image Problem" });
                    }

                    int ProductID = int.Parse(systemCode.Split('%')[0]);

                    string query = "INSERT INTO ProductList (ProductId, ProductName, ProductGroupID,Specification,UnitId, ImagePath, ProductSubName, IsActive, AddedDate, AddedBy, AddedPC)" +
                        "VALUES (@ProductId, @ProductName, @ProductGroupID, @Specification, @UnitId, @ImagePath, @ProductSubName, @IsActive, @AddedDate, @AddedBy, @AddedPC)";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@ProductId", ProductID);
                    cmd.Parameters.AddWithValue("@ProductName", productListDto.ProductName);
                    cmd.Parameters.AddWithValue("@ProductGroupID", productListDto.ProductGroupID);
                    cmd.Parameters.AddWithValue("@Specification", productListDto.Specification);
                    cmd.Parameters.AddWithValue("@UnitId", productListDto.UnitId);
                    cmd.Parameters.AddWithValue("@ImagePath", ImagePath);
                    cmd.Parameters.AddWithValue("@ProductSubName", productListDto.ProductSubName ?? string.Empty);
                    cmd.Parameters.AddWithValue("@IsActive", 1);
                    cmd.Parameters.AddWithValue("@AddedBy", productListDto.AddedBy);
                    cmd.Parameters.AddWithValue("@AddedDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@AddedPC", productListDto.AddedPC);

                    await con.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                    await con.CloseAsync();

                    return Ok(new { message = "Product Added Successfully." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        //=================================================

        [HttpPut("UpdateProductList")]
        public async Task<IActionResult> UpdateProductListAsync([FromForm] ProductListDto productListDto)
        {
            try
            {
                Boolean check = await ProductListExist(productListDto.ProductId);

                if (check)
                {
                    await con.OpenAsync();

                    using (SqlTransaction transaction = con.BeginTransaction())
                    {
                        try
                        {
                            string ImagePath = CommonServices.UploadFiles(foldername, filename, productListDto.ImageFile);

                            if (ImagePath != null)
                            {


                                if (string.IsNullOrEmpty(productListDto.ExistingImageFileName))
                                {
                                    string query = "UpdateProductListWithImage";
                                    SqlCommand cmd = new SqlCommand(query, con, transaction);
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.AddWithValue("@ProductId", productListDto.ProductId ?? (object)DBNull.Value);
                                    cmd.Parameters.AddWithValue("@ProductName", productListDto.ProductName);
                                    cmd.Parameters.AddWithValue("@ProductGroupID", productListDto.ProductGroupID);
                                    cmd.Parameters.AddWithValue("@Specification", productListDto.Specification);
                                    cmd.Parameters.AddWithValue("@UnitId", productListDto.UnitId);

                                    cmd.Parameters.AddWithValue("@ImagePath", ImagePath);
                                    cmd.Parameters.AddWithValue("@ProductSubName", productListDto.ProductSubName ?? string.Empty);

                                    cmd.Parameters.AddWithValue("@UpdatedBy", productListDto.UpdatedBy ?? string.Empty);
                                    cmd.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);
                                    cmd.Parameters.AddWithValue("@UpdatedPC", productListDto.UpdatedPC ?? string.Empty);

                                    await cmd.ExecuteNonQueryAsync();
                                }
                            }
                            else
                            {
                                string query1 = "UpdateProductListWithoutImage";
                                SqlCommand cmdd = new SqlCommand(query1, con, transaction);
                                cmdd.CommandType = CommandType.StoredProcedure;
                                cmdd.Parameters.AddWithValue("@ProductId", productListDto.ProductId ?? (object)DBNull.Value);
                                cmdd.Parameters.AddWithValue("@ProductName", productListDto.ProductName);
                                cmdd.Parameters.AddWithValue("@ProductGroupID", productListDto.ProductGroupID);
                                cmdd.Parameters.AddWithValue("@Specification", productListDto.Specification);
                                cmdd.Parameters.AddWithValue("@UnitId", productListDto.UnitId);
                                cmdd.Parameters.AddWithValue("@ProductSubName", productListDto.ProductSubName ?? string.Empty);
                                cmdd.Parameters.AddWithValue("@UpdatedBy", productListDto.UpdatedBy ?? string.Empty);
                                cmdd.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);
                                cmdd.Parameters.AddWithValue("@UpdatedPC", productListDto.UpdatedPC ?? string.Empty);

                                await cmdd.ExecuteNonQueryAsync();


                            }

                            transaction.Commit();
                            return Ok(new { message = "Product Group updated successfully." });
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            return BadRequest(new { message = $"Error updating product group: {ex.Message}" });
                        }
                        finally
                        {
                            con.Close();  
                        }
                    }
                }
                else
                {
                    return NotFound(new { message = "Product Group not found!" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error updating product group: {ex.Message}" });
            }
        }

        //=================================================

        [HttpGet]
        [Route("GetProductList")]
        public async Task<IActionResult> GetProductGroupsListAsync()
        {
            try
            {
                List<GetProductListDto> lst = new List<GetProductListDto>();
                await con.OpenAsync();
                string query = @"SELECT
                                PL.ProductId, 
                                PL.ProductName, 
                                PL.UnitId,
                                U.Name as UnitName, 
                                PL.ProductGroupID,
                                PG.ProductGroupName
                                FROM ProductList PL
                                JOIN Units U ON U.UnitId = PL.UnitId
                                JOIN ProductGroups PG ON PG.ProductGroupID = PL.ProductGroupID 
                                WHERE PL.IsActive = 1  
                                ORDER BY PL.ProductId DESC;  ";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            GetProductListDto modelObj = new GetProductListDto();
                            modelObj.ProductId = Convert.ToInt32(reader["ProductId"]);
                            modelObj.ProductName = reader["ProductName"].ToString();
                            modelObj.UnitId = Convert.ToInt32(reader["UnitId"]);
                            modelObj.UnitName = reader["UnitName"].ToString();
                            modelObj.ProductGroupId = Convert.ToInt32(reader["ProductGroupID"]);
                            modelObj.ProductGroupName = reader["ProductGroupName"].ToString();



                            lst.Add(modelObj);
                        }
                    }
                }

                return Ok(lst);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }


        [HttpGet]
        [Route("GetProductListByStatus")]
        public async Task<IActionResult> GetProductListByStatus(Int32? status = null)
        {
            try
            {
                List<GetProductListByStatusDto> lst = new List<GetProductListByStatusDto>();
                await con.OpenAsync();
                string query = "";
                if (status != null)
                {
                    query = @"SELECT ProductId,PL.ProductName,PL.ProductGroupID,PG.ProductGroupName,PL.Specification,PL.UnitId,U.Name Unit,PL.IsActive,PL.AddedDate,PL.UpdatedDate,PL.AddedBy,PL.UpdatedBy,PL.AddedPC,PL.UpdatedPC,PL.ImagePath,PL.Status,ProductSubName FROM ProductList PL LEFT JOIN ProductGroups PG ON PL.ProductGroupID=PG.ProductGroupID LEFT JOIN Units U ON PL.UnitId = U.UnitId WHERE PL.IsActive= @IsActive ORDER BY ProductId  DESC;";
                }
                else
                {
                    query = @"SELECT ProductId,PL.ProductName,PL.ProductGroupID,PG.ProductGroupName,PL.Specification,PL.UnitId,U.Name Unit,PL.IsActive,PL.AddedDate,PL.UpdatedDate,PL.AddedBy,PL.UpdatedBy,PL.AddedPC,PL.UpdatedPC,PL.ImagePath,PL.Status,ProductSubName FROM ProductList PL LEFT JOIN ProductGroups PG ON PL.ProductGroupID=PG.ProductGroupID LEFT JOIN Units U ON PL.UnitId = U.UnitId WHERE CONVERT(DATE, AddedDate) = CONVERT(DATE, GETDATE()) ORDER BY ProductId  DESC";
                }
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    if (status != null)
                    {
                        cmd.Parameters.Add(new SqlParameter("@IsActive", status));
                    }
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            GetProductListByStatusDto modelObj = new GetProductListByStatusDto();
                            modelObj.ProductId = Convert.ToInt32(reader["ProductId"]);
                            modelObj.UnitId = Convert.ToInt32(reader["UnitId"]);
                            modelObj.Unit = reader["Unit"].ToString();
                            modelObj.ProductGroupID = Convert.ToInt32(reader["ProductGroupID"]);
                            modelObj.ProductGroupName = reader["ProductGroupName"].ToString();
                            modelObj.ProductName = reader["ProductName"].ToString();
                            modelObj.Specification = reader["Specification"].ToString();
                            modelObj.ImagePath = reader["ImagePath"].ToString();
                            modelObj.ProductSubName = reader["ProductSubName"].ToString();
                            modelObj.IsActive = reader["IsActive"] is DBNull ? (bool?)null : (bool)reader["IsActive"];
                            modelObj.AddedDate = reader["AddedDate"] is DBNull ? (DateTime?)null : (DateTime)reader["AddedDate"];
                            
                            lst.Add(modelObj);
                        }
                    }
                }
                return Ok(lst);
            }

            catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        // ==============================productName by productGroupId===================

        [HttpGet]
        [Route("GetProductNameByProductGroupId")]
        public async Task<List<ProductNameByGroup>> GetProductNameByProductGroupId(int ProductGroupId)
        {
            List<ProductNameByGroup> lst = new List<ProductNameByGroup>();

            try
            {
                await con.OpenAsync();
                string query = @"SELECT p.ProductName, g.ProductGroupName, p.ProductGroupId, p.ProductId, U.Name 
                 FROM ProductList p 
                 INNER JOIN ProductGroups g ON p.ProductGroupId = g.ProductGroupID 
				 left JOIN Units U ON  U.UnitId = p.UnitId
                 WHERE p.ProductGroupId = @ProductGroupId;";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    // Add the parameter and its value to the command
                    cmd.Parameters.AddWithValue("@ProductGroupId", ProductGroupId);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            ProductNameByGroup modelObj = new ProductNameByGroup();
                            modelObj.ProductGroupId = Convert.ToInt32(reader["ProductGroupID"]);
                            modelObj.ProductId = Convert.ToInt32(reader["ProductId"]);
                            modelObj.ProductGroupName = reader["ProductGroupName"].ToString();
                            modelObj.ProductName = reader["ProductName"].ToString();
                            modelObj.UnitName = reader["Name"].ToString();

                            lst.Add(modelObj);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                // You might want to throw the exception again if you cannot handle it at this level.
                throw;
            }
            finally
            {
                // Ensure the connection is closed, even in case of an exception.
                if (con.State == ConnectionState.Open)
                {
                    await con.CloseAsync();
                }
            }

            return lst;
        }

        //========================tushar=========================

        [HttpPut("MakeProductActiveOrInactive")]


        public async Task<IActionResult> MakeProductActiveOrInactiveAsync(List<int> productIds, bool? IsActive)
        {
            try
            {
                string query = @"UPDATE ProductList
                          SET IsActive = @IsActive
                          WHERE ProductId IN ({0})";

                // Create a parameterized list of parameters for the IN clause
                string parameterList = string.Join(",", productIds.Select((_, index) => $"@ProductId{index}"));
                query = string.Format(query, parameterList);

                using (SqlCommand command = new SqlCommand(query, con))
                {
                    // Add parameters for productIds
                    for (int i = 0; i < productIds.Count; i++)
                    {
                        command.Parameters.AddWithValue($"@ProductId{i}", productIds[i]);
                    }

                    command.Parameters.AddWithValue("@IsActive", IsActive);

                    await con.OpenAsync();

                    // Execute the command
                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        return BadRequest(new { message = $"No products found." });
                    }

                    await con.CloseAsync();
                }

                return Ok(new { message = $"Products' IsActive status changed." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Products' IsActive status not changed: {ex.Message}" });
            }
        }

    }
}
