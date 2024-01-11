using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NDE_Digital_Market.Model;
using NDE_Digital_Market.DTOs;
using System.Data;
using System.Data.SqlClient;
using NDE_Digital_Market.SharedServices;

namespace NDE_Digital_Market.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductGroupsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly SqlConnection con;
        private readonly string foldername;
        private readonly string filename = "SellerProductGroup";
        public ProductGroupsController(IConfiguration configuration)
        {
            CommonServices commonServices = new CommonServices(configuration);
            _configuration = configuration;
            con = new SqlConnection(_configuration.GetConnectionString("HealthCare"));
            foldername = commonServices.FilesPath + "SellerProductGroupFiles";
        }

        private async Task<Boolean> ProductGroupsNameCheck(string productgoodsname)
        {
            string query = @"SELECT COUNT(*) FROM  [ProductGroups] WHERE ProductGroupName = @productgoodsname";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@productgoodsname", productgoodsname);
            await con.OpenAsync();
            int count = (int) await cmd.ExecuteScalarAsync();
            await con.CloseAsync();
            Boolean check = false;
            if (count > 0)
            {
                check = true;
            }
            return check;
        }

        private async Task<Boolean> ProductGroupsExist(int? productGroupID)
        {
            if (productGroupID.HasValue)
            {
                string query = @"SELECT COUNT(*) FROM ProductGroups WHERE ProductGroupID = @ProductGroupID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@ProductGroupID", productGroupID.Value);
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



        [HttpPost("CreateProductGroups")]
        public async Task<IActionResult> CreateProductGroupsAsync([FromForm]  ProductGroupsDto productGroupsDto)
        {
            try
            {
                Boolean check = await ProductGroupsNameCheck(productGroupsDto.ProductGroupName);
                if (check)
                {
                    return BadRequest(new { message = "Product GroupName already exists!" });
                }
                else
                {
                    string systemCode = string.Empty;

      
                    SqlCommand cmdSP = new SqlCommand("spMakeSystemCode", con);
                    {
                        cmdSP.CommandType = CommandType.StoredProcedure;
                        cmdSP.Parameters.AddWithValue("@TableName", "ProductGroups");
                        cmdSP.Parameters.AddWithValue("@Date", DateTime.Now.ToString("yyyy-MM-dd"));
                        cmdSP.Parameters.AddWithValue("@AddNumber", 1);

                        await con.OpenAsync();
                        var tempSystem = await cmdSP.ExecuteScalarAsync();
                        systemCode = tempSystem?.ToString() ?? string.Empty;
                        await con.CloseAsync();
                    }

                    int ProductGroupsID = int.Parse(systemCode.Split('%')[0]);
                    string ProductGroupsCode = systemCode.Split('%')[1];



                    string ImagePath = CommonServices.UploadFiles(foldername, filename, productGroupsDto.ImageFile);
                    if (ImagePath == null)
                    {
                        return BadRequest(new { message = "Image Problem" });
                    }
                    string query = @"INSERT INTO ProductGroups (ProductGroupID, ProductGroupCode, ProductGroupName,ImagePath, ProductGroupPrefix, ProductGroupDetails, IsActive, AddedBy, DateAdded, AddedPC)
                        VALUES(@ProductGroupID, @ProductGroupCode, @ProductGroupName, @ImagePath, @ProductGroupPrefix, @ProductGroupDetails, @IsActive, @AddedBy, @DateAdded, @AddedPC);";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@ProductGroupID", ProductGroupsID);
                    cmd.Parameters.AddWithValue("@ProductGroupCode", ProductGroupsCode);
                    cmd.Parameters.AddWithValue("@ProductGroupName", productGroupsDto.ProductGroupName);
                    cmd.Parameters.AddWithValue("@ImagePath", ImagePath);
                    cmd.Parameters.AddWithValue("@ProductGroupPrefix", productGroupsDto.ProductGroupPrefix);
                    cmd.Parameters.AddWithValue("@ProductGroupDetails", productGroupsDto.ProductGroupDetails ?? string.Empty);
                    cmd.Parameters.AddWithValue("@IsActive", 1);
                    cmd.Parameters.AddWithValue("@AddedBy", productGroupsDto.AddedBy);
                    cmd.Parameters.AddWithValue("@DateAdded", DateTime.Now);
                    cmd.Parameters.AddWithValue("@AddedPC", productGroupsDto.AddedPC);

                    await con.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                    await con.CloseAsync();

                    return Ok(new { message = "Product Group Create successfully." });
                }


            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        //==================================================

        [HttpPut("UpdateProductGroups")]
        public async Task<IActionResult> UpdateProductGroupsAsync([FromForm] ProductGroupsDto productGroupsDto)
        {
            try
            {
                Boolean check = await ProductGroupsExist(productGroupsDto.ProductGroupID);

                if (check)
                {
                    await con.OpenAsync();

                    using (SqlTransaction transaction = con.BeginTransaction())
                    {
                        try
                        {
                            string ImagePath = CommonServices.UploadFiles(foldername, filename, productGroupsDto.ImageFile);

                            if (ImagePath != null)
                            {


                                if (string.IsNullOrEmpty(productGroupsDto.ExistingImageFileName))
                                {
                                    string query = "UpdateProductGroupWithImage";
                                    SqlCommand cmd = new SqlCommand(query, con, transaction);
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.AddWithValue("@ProductGroupID", productGroupsDto.ProductGroupID);
                                    cmd.Parameters.AddWithValue("@ProductGroupName", productGroupsDto.ProductGroupName);
                                    cmd.Parameters.AddWithValue("@ImagePath", ImagePath);
                                    cmd.Parameters.AddWithValue("@ProductGroupPrefix", productGroupsDto.ProductGroupPrefix);
                                    cmd.Parameters.AddWithValue("@ProductGroupDetails", productGroupsDto.ProductGroupDetails ?? string.Empty);
                                    cmd.Parameters.AddWithValue("@UpdatedBy", productGroupsDto.UpdatedBy);
                                    cmd.Parameters.AddWithValue("@DateUpdated", DateTime.Now);
                                    cmd.Parameters.AddWithValue("@UpdatedPC", productGroupsDto.UpdatedPC);

                                    await cmd.ExecuteNonQueryAsync();
                                }
                            }
                            else
                            {
                                string query1 = "UpdateProductGroupWithOutImage";
                                SqlCommand cmdd = new SqlCommand(query1, con, transaction);
                                cmdd.CommandType = CommandType.StoredProcedure;
                                cmdd.Parameters.AddWithValue("@ProductGroupID", productGroupsDto.ProductGroupID);
                                cmdd.Parameters.AddWithValue("@ProductGroupName", productGroupsDto.ProductGroupName);
                                cmdd.Parameters.AddWithValue("@ProductGroupPrefix", productGroupsDto.ProductGroupPrefix);
                                cmdd.Parameters.AddWithValue("@ProductGroupDetails", productGroupsDto.ProductGroupDetails ?? string.Empty);
                                cmdd.Parameters.AddWithValue("@UpdatedBy", productGroupsDto.UpdatedBy);
                                cmdd.Parameters.AddWithValue("@DateUpdated", DateTime.Now);
                                cmdd.Parameters.AddWithValue("@UpdatedPC", productGroupsDto.UpdatedPC);

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

        //==================================================

        [HttpGet]
        [Route("GetProductGroupsList")]
        public async Task<List<ProductGroupsModel>> GetProductGroupsListAsync()
        {
            List<ProductGroupsModel> lst = new List<ProductGroupsModel>();

            try
            {
                await con.OpenAsync();
                string query = @"SELECT [ProductGroupID],[ProductGroupCode],[ProductGroupName],[ProductGroupPrefix],[ProductGroupDetails],
            [IsActive] FROM ProductGroups WHERE IsActive = 1 ORDER BY [ProductGroupID] DESC;";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            ProductGroupsModel modelObj = new ProductGroupsModel();
                            modelObj.ProductGroupID = Convert.ToInt32(reader["ProductGroupID"]);
                            modelObj.ProductGroupCode = reader["ProductGroupCode"].ToString();
                            modelObj.ProductGroupName = reader["ProductGroupName"].ToString();
                            modelObj.ProductGroupPrefix = reader["ProductGroupPrefix"].ToString();
                            modelObj.ProductGroupDetails = reader["ProductGroupDetails"].ToString();
                            modelObj.IsActive = Convert.ToBoolean(reader["IsActive"]);

                            lst.Add(modelObj);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }

            return lst;
        }

        [HttpGet]
        [Route("GetProductGroupsListByStatus")]
        public async Task<List<ProductGroupByStatusDTO>> GetProductGroupsListByStatus(Int32? status = null)
        {
            List<ProductGroupByStatusDTO> lst = new List<ProductGroupByStatusDTO>();

            try
            {
                await con.OpenAsync();

                string query = "";
                if (status != null)
                {
                    query = @"SELECT * FROM ProductGroups WHERE IsActive= @IsActive ORDER BY ProductGroupID  DESC;";
                }
                else
                {
                    query = @"SELECT * FROM ProductGroups WHERE CONVERT(DATE, DateAdded) = CONVERT(DATE, GETDATE()) ORDER BY ProductGroupID  DESC";
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
                            ProductGroupByStatusDTO modelObj = new ProductGroupByStatusDTO();
                            modelObj.ProductGroupID = Convert.ToInt32(reader["ProductGroupID"]);
                            modelObj.ProductGroupCode = reader["ProductGroupCode"].ToString();
                            modelObj.ProductGroupName = reader["ProductGroupName"].ToString();
                            modelObj.ProductGroupPrefix = reader["ProductGroupPrefix"].ToString();
                            modelObj.ProductGroupDetails = reader["ProductGroupDetails"].ToString();
                            modelObj.IsActive = Convert.ToBoolean(reader["IsActive"]);
                            modelObj.Imagepath = reader["Imagepath"].ToString();
                            modelObj.DateAdded = reader.IsDBNull(reader.GetOrdinal("DateAdded")) ? (DateTime?)null : (DateTime?)reader["DateAdded"];

                            lst.Add(modelObj);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
            return lst;
        }


        //========================tushar=========================

        [HttpPut("MakeGroupActiveOrInactive")]
        public async Task<IActionResult> MakeGroupActiveOrInactiveAsync(int? groupId, bool? IsActive)
        {
            try
            {
                string query = @"UPDATE ProductGroups
                                    SET IsActive = @IsActive
                                    WHERE ProductGroupID = @groupId";
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    command.Parameters.AddWithValue("@IsActive", IsActive);
                    command.Parameters.AddWithValue("@groupId", groupId);

                    await con.OpenAsync();
                    // Execute the command
                    int Res = await command.ExecuteNonQueryAsync();
                    if (Res == 0)
                    {
                        return BadRequest(new { message = $"Group didnot found." });
                    }
                    await con.CloseAsync();
                }
                return Ok(new { message = $"Group IsActive status changed." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Group IsActive status not change : {ex.Message}" });
            }
        }

    }
}
