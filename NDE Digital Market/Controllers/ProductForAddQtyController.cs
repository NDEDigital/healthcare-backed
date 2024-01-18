using Microsoft.AspNetCore.Mvc;
using NDE_Digital_Market.SharedServices;
using NDE_Digital_Market.Model;
using NDE_Digital_Market.DTOs;
using System.Data.SqlClient;
using System.Data;

namespace NDE_Digital_Market.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductQuantityController : Controller
    {

        private readonly string foldername;
        private readonly string filename = "SellerProductPriceAndOffer";
        private CommonServices _commonServices;
        private readonly string _healthCareConnection;
        private readonly IConfiguration configuration;
        private readonly SqlConnection con;

        public ProductQuantityController(IConfiguration config)
        {
            _commonServices = new CommonServices(config);
            _healthCareConnection = _commonServices.HealthCareConnection;
            configuration = config;
            con = new SqlConnection(_healthCareConnection);
            CommonServices commonServices = new CommonServices(configuration);
            foldername = commonServices.FilesPath + "SellerProductPriceAndOfferFiles";
        }

        [HttpGet]
        [Route("ProductGroupsDropdownByUserId/{userID}")]
        public async Task<IActionResult> ProductGroupsDropdownByUserId(int userID)
        {
            var productGroupsDropdownByUserId = new List<ProductGroupsDropdown>();
            try
            {
                using (var connection = new SqlConnection(_healthCareConnection))
                {
                    using (var command = new SqlCommand("GetProductGroupsDropdownByUserId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@userID", userID));
                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var productGroupsDropdown = new ProductGroupsDropdown();
                                productGroupsDropdown.ProductGroupID = Convert.ToInt32(reader["ProductGroupID"]);
                                productGroupsDropdown.ProductGroupName = reader["ProductGroupName"].ToString();
                                productGroupsDropdownByUserId.Add(productGroupsDropdown);
                            }
                        }
                    }
                }
                return Ok(productGroupsDropdownByUserId);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving companies: " + ex.Message);
            }
        }


        [HttpGet("GetProductForAddQtyByUserId/{UserId}/{productGroupId}")]
        public async Task<IActionResult> GetProductForAddQtyByUserId(int UserId, int productGroupId)
        {
            var products = new List<SellerPoductListModel>();

            try
            {
                using (var connection = new SqlConnection(_healthCareConnection))
                {
                    using (var command = new SqlCommand("GetProductForAddQtyByUserId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@UserId", UserId));
                        command.Parameters.Add(new SqlParameter("@productGroupId", productGroupId));
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
                                    UnitId = reader.IsDBNull(reader.GetOrdinal("UnitId")) ? 0 : reader.GetInt32(reader.GetOrdinal("UnitId")),
                                    Unit = reader.IsDBNull(reader.GetOrdinal("Unit")) ? null : reader.GetString(reader.GetOrdinal("Unit")),
                                    Price = reader.IsDBNull(reader.GetOrdinal("Price")) ? 0 : reader.GetDecimal(reader.GetOrdinal("Price")),
                                    AvailableQty = reader.IsDBNull(reader.GetOrdinal("AvailableQty")) ? 0 : reader.GetDecimal(reader.GetOrdinal("AvailableQty"))
                                };
                                products.Add(product);
                            }
                        }
                    }
                }

                //if (products.Count == 0)
                //{
                //    return NotFound("No products found for the given user ID.");
                //}

                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving products: " + ex.Message);
            }


            return Ok();
        }



        [HttpPost("PortalReceivedPost")]
        public async Task<IActionResult> InsertPortalReceivedAsync(PortalReceivedMasterDto portaldata)
        {

            SqlTransaction transaction = null;
            try
            {
                string systemCode = string.Empty;
                await con.OpenAsync();
                transaction = con.BeginTransaction();

                SqlCommand cmdSP = new SqlCommand("spMakeSystemCode", con, transaction);
                {
                    cmdSP.CommandType = CommandType.StoredProcedure;
                    cmdSP.Parameters.AddWithValue("@TableName", "PortalReceivedMaster");
                    cmdSP.Parameters.AddWithValue("@Date", DateTime.Now.ToString("yyyy-MM-dd"));
                    cmdSP.Parameters.AddWithValue("@AddNumber", 1);

                    var tempSystem = await cmdSP.ExecuteScalarAsync();
                    systemCode = tempSystem?.ToString() ?? string.Empty;
                }

                int PortalReceivedId = int.Parse(systemCode.Split('%')[0]);
                string PortalReceivedCode = systemCode.Split('%')[1];
                portaldata.PortalReceivedId = PortalReceivedId;
                portaldata.PortalReceivedCode = PortalReceivedCode;

                SqlCommand cmd = new SqlCommand("InsertPortalReceivedMaster", con, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@PortalReceivedId", PortalReceivedId);
                cmd.Parameters.AddWithValue("@PortalReceivedCode", PortalReceivedCode);
                cmd.Parameters.AddWithValue("@MaterialReceivedDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@ChallanNo", portaldata.ChallanNo ?? String.Empty);
                cmd.Parameters.AddWithValue("@ChallanDate", portaldata.ChallanDate ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Remarks", portaldata.Remarks ?? String.Empty);
                cmd.Parameters.AddWithValue("@UserId", portaldata.UserId);
                cmd.Parameters.AddWithValue("@AddedBy", portaldata.AddedBy);
                cmd.Parameters.AddWithValue("@AddedDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@AddedPC", portaldata.AddedPC);

                int a = await cmd.ExecuteNonQueryAsync();

                if (a > 0)
                {
                    var detailsResult = await InsertPortalReceivedAsync(PortalReceivedId, portaldata.PortalReceivedDetailslist, transaction);
                    if (detailsResult is BadRequestObjectResult)
                    {
                        throw new Exception((detailsResult as BadRequestObjectResult).Value.ToString());
                    }
                }
                else
                {
                    return BadRequest(new { message = "Portal Master data isn't Inserted Successfully." });
                }
                transaction.Commit();
                return Ok(portaldata);
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    transaction.Rollback();
                }
                return BadRequest(ex.Message);
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    await con.CloseAsync();
                }
            }
        }

        private async Task<IActionResult> InsertPortalReceivedAsync(int PortalReceivedId, List<PortalReceivedDetailsDto> PortalReceivedDetailsList, SqlTransaction transaction)
        {
            try
            {
                for (int i = 0; i < PortalReceivedDetailsList.Count; i++)
                {
                    PortalReceivedDetailsList[i].PortalReceivedId = PortalReceivedId;
                    string query = "InsertPortalReceivedDetails";
                    SqlCommand CheckCMD = new SqlCommand(query, con, transaction);
                    CheckCMD.CommandType = CommandType.StoredProcedure;
                    CheckCMD.Parameters.Clear();
                    CheckCMD.Parameters.AddWithValue("@PortalReceivedId", PortalReceivedId);
                    CheckCMD.Parameters.AddWithValue("@ProductGroupID", PortalReceivedDetailsList[i].ProductGroupId);
                    CheckCMD.Parameters.AddWithValue("@ProductId", PortalReceivedDetailsList[i].ProductId);
                    CheckCMD.Parameters.AddWithValue("@Specification", PortalReceivedDetailsList[i].Specification);
                    CheckCMD.Parameters.AddWithValue("@ReceivedQty", PortalReceivedDetailsList[i].ReceivedQty);
                    CheckCMD.Parameters.AddWithValue("@UnitId", PortalReceivedDetailsList[i].UnitId);
                    CheckCMD.Parameters.AddWithValue("@Price", PortalReceivedDetailsList[i].Price);
                    CheckCMD.Parameters.AddWithValue("@TotalPrice", PortalReceivedDetailsList[i].TotalPrice);
                    CheckCMD.Parameters.AddWithValue("@UserId", PortalReceivedDetailsList[i].UserId);
                    CheckCMD.Parameters.AddWithValue("@Remarks", PortalReceivedDetailsList[i].Remarks ?? String.Empty);
                    CheckCMD.Parameters.AddWithValue("@AddedBy", PortalReceivedDetailsList[i].AddedBy);
                    CheckCMD.Parameters.AddWithValue("@DateAdded", DateTime.Now);
                    CheckCMD.Parameters.AddWithValue("@AddedPC", PortalReceivedDetailsList[i].AddedPC);
                    await CheckCMD.ExecuteNonQueryAsync();

                }


                // transaction.Commit();



                return Ok(new { message = "Portal Details data Inserted Successfully." });
            }
            catch (Exception ex)
            {


                return BadRequest(ex.Message);
            }
        }

        private async Task<Boolean> SellerProductPriceAndOfferCheck(int ProductId, int? userId)
        {

            string query = @"SELECT COUNT(*) AS ProductCount FROM SellerProductPriceAndOffer WHERE ProductId = @ProductId AND CompanyCode =(SELECT UPPER(CompanyCode) FROM UserRegistration WHERE UserId = @UserId)";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@ProductId", ProductId);
            cmd.Parameters.AddWithValue("@UserId", userId);
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




        [HttpPost("CreateSellerProductPriceAndOffer")]
        public async Task<IActionResult> CreateSellerProductPriceAndOfferAsync([FromForm] SellerProductPriceAndOfferDto sellerproductdata)
        {
            try
            {
                Boolean ProductPriceAndOfferExist = await SellerProductPriceAndOfferCheck(sellerproductdata.ProductId, sellerproductdata.UserId);
                if (ProductPriceAndOfferExist)
                {
                    return BadRequest(new { message = "ProductPriceAndOffer Allready Added." });
                }
                else
                {
                    string ImagePath = CommonServices.UploadFiles(foldername, filename, sellerproductdata.ImageFile);

                    string query = @"INSERT INTO SellerProductPriceAndOffer(ProductId, UserId, Price,DiscountAmount,DiscountPct,EffectivateDate,
                    EndDate,ImagePath,Status,IsActive, AddedDate,AddedBy,AddedPC,TotalPrice,CompanyCode) 
                    VALUES (@ProductId,@UserId,@Price,@DiscountAmount,@DiscountPct,@EffectivateDate,@EndDate, @ImagePath,
                    @Status, @IsActive, @AddedDate,@AddedBy, @AddedPC,@TotalPrice, (SELECT UPPER(CompanyCode) FROM UserRegistration WHERE UserId = @UserId));";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@ProductId", sellerproductdata.ProductId);
                    cmd.Parameters.AddWithValue("@UserId", sellerproductdata.UserId);
                    cmd.Parameters.AddWithValue("@Price", sellerproductdata.Price);
                    cmd.Parameters.AddWithValue("@DiscountAmount", sellerproductdata.DiscountAmount ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@DiscountPct", sellerproductdata.DiscountPct ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EffectivateDate", sellerproductdata.EffectivateDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EndDate", sellerproductdata.EndDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ImagePath", ImagePath);
                    cmd.Parameters.AddWithValue("@Status", "Pending");
                    cmd.Parameters.AddWithValue("@IsActive", 1);
                    cmd.Parameters.AddWithValue("@TotalPrice", sellerproductdata.TotalPrice);

                    cmd.Parameters.AddWithValue("@AddedBy", sellerproductdata.AddedBy);
                    cmd.Parameters.AddWithValue("@AddedDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@AddedPC", sellerproductdata.AddedPC);

                    await con.OpenAsync();
                    int res = await cmd.ExecuteNonQueryAsync();
                    await con.CloseAsync();
                    if (res > 0)
                    {
                        return Ok(new { message = "SellerProductPriceAndOffer Added Successfully." });
                    }
                    else
                    {
                        return BadRequest(new { message = "SellerProductPriceAndOffer Add Unsuccessfull." });
                    }
                }



            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

       

        [HttpPut("UpdateSellerProductPriceAndOffer")]
        public async Task<IActionResult> UpdateSellerProductPriceAndOffer([FromForm] SellerProductPriceAndOfferDto sellerproductdata)
        {
            try
            {
                // Validation
                if (sellerproductdata == null)
                {
                    return BadRequest(new { message = "Invalid request data." });
                }

                // Additional validation as needed for required fields, e.g., ProductId, UserId, Price, etc.

                Boolean check = await SellerProductPriceAndOfferCheck(sellerproductdata.ProductId, sellerproductdata.UserId);

                if (check)
                {
                    await con.OpenAsync();

                    using (SqlTransaction transaction = con.BeginTransaction())
                    {
                        try
                        {
                            
                            string ImagePath = CommonServices.UploadFiles(foldername, filename, sellerproductdata.ImageFile);

                            string query = "UpdateSellerProductPriceAndOffer";
                            SqlCommand cmd = new SqlCommand(query, con, transaction);
                            cmd.CommandType = CommandType.StoredProcedure;


                            if (ImagePath != null)
                            {
                                // Adding parameters with null checks
                                cmd.Parameters.AddWithValue("@ProductId", sellerproductdata.ProductId);
                                cmd.Parameters.AddWithValue("@UserId", sellerproductdata.UserId);
                                cmd.Parameters.AddWithValue("@Price", sellerproductdata.Price ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@DiscountAmount", sellerproductdata.DiscountAmount ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@DiscountPct", sellerproductdata.DiscountPct ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@EffectivateDate", sellerproductdata.EffectivateDate ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@EndDate", sellerproductdata.EndDate ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@Status", sellerproductdata.Status ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@IsActive", sellerproductdata.IsActive ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@UpdatedBy", sellerproductdata.UpdatedBy ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@UpdatedDate", sellerproductdata.UpdatedDate ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@UpdatedPC", sellerproductdata.UpdatedPC ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@TotalPrice", sellerproductdata.TotalPrice ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@ImagePath", ImagePath);

                                await cmd.ExecuteNonQueryAsync();
                            }
                            else
                            {
                                // Adding parameters with null checks
                                cmd.Parameters.AddWithValue("@ProductId", sellerproductdata.ProductId);
                                cmd.Parameters.AddWithValue("@UserId", sellerproductdata.UserId);
                                cmd.Parameters.AddWithValue("@Price", sellerproductdata.Price ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@DiscountAmount", sellerproductdata.DiscountAmount ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@DiscountPct", sellerproductdata.DiscountPct ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@EffectivateDate", sellerproductdata.EffectivateDate ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@EndDate", sellerproductdata.EndDate ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@Status", sellerproductdata.Status ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@IsActive", sellerproductdata.IsActive ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@UpdatedBy", sellerproductdata.UpdatedBy ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@UpdatedDate", sellerproductdata.UpdatedDate ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@UpdatedPC", sellerproductdata.UpdatedPC ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@TotalPrice", sellerproductdata.TotalPrice ?? (object)DBNull.Value);

                                await cmd.ExecuteNonQueryAsync();
                            }



                            transaction.Commit();
                            return Ok(new { message = "Price updated successfully." });
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            return BadRequest(new { message = $"Error updating price: {ex.Message}" });
                        }
                        finally
                        {
                            con.Close();
                        }
                    }
                }
                else
                {
                    return NotFound(new { message = "Price not found!" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error updating price: {ex.Message}" });
            }
        }



        [HttpGet]
        [Route("GetSellerProductsByCompanyCode")]
        public async Task<IActionResult> GetSellerProductsByCompanyCode(string userID, Int32? status = null)
        {
            var sellerProductsByCompanyCode = new List<SellerProductsByCompanyCodeDto>();
            try
            {
                using (var connection = new SqlConnection(_healthCareConnection))
                {
                    using (var command = new SqlCommand("GetSellerProductsByCompanyCode", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@userID", userID));
                        command.Parameters.Add(new SqlParameter("@status", status));
                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var sellerProduct = new SellerProductsByCompanyCodeDto();

                                sellerProduct.SellerProductId = Convert.ToInt32(reader["SellerProductId"]);
                                sellerProduct.ProductId = Convert.ToInt32(reader["ProductId"]);
                                sellerProduct.ProductName = reader["ProductName"].ToString();
                                sellerProduct.UserId = reader["UserId"].ToString();
                                sellerProduct.FullName = reader["FullName"].ToString();
                                sellerProduct.Price = reader["Price"] != DBNull.Value ? Convert.ToDecimal(reader["Price"]) : 0;
                                sellerProduct.DiscountAmount = reader["DiscountAmount"] != DBNull.Value ? Convert.ToDecimal(reader["DiscountAmount"]) : 0;
                                sellerProduct.DiscountPct = reader["DiscountPct"] != DBNull.Value ? Convert.ToDecimal(reader["DiscountPct"]) : 0;
                                sellerProduct.EffectivateDate = reader["EffectivateDate"] != DBNull.Value ? Convert.ToDateTime(reader["EffectivateDate"]) : DateTime.MinValue;
                                sellerProduct.EndDate = reader["EndDate"] != DBNull.Value ? Convert.ToDateTime(reader["EndDate"]) : DateTime.MinValue;
                                sellerProduct.ImagePath = reader["ImagePath"].ToString();
                                sellerProduct.Status = reader["Status"].ToString();
                                sellerProduct.IsActive = reader["IsActive"] != DBNull.Value ? Convert.ToBoolean(reader["IsActive"]) : false;
                                sellerProduct.TotalPrice = reader["TotalPrice"] != DBNull.Value ? Convert.ToDecimal(reader["TotalPrice"]) : 0;
                                sellerProduct.AddedDate = reader["AddedDate"] != DBNull.Value ? Convert.ToDateTime(reader["AddedDate"]) : DateTime.MinValue;
                                sellerProduct.UnitName = reader["UnitName"].ToString();


                                sellerProductsByCompanyCode.Add(sellerProduct);
                            }

                        }
                    }
                }
                return Ok(sellerProductsByCompanyCode);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving companies: " + ex.Message);
            }
        }




        private async Task<IActionResult> GetPortalReceivedDetailsAndMasterAsync(int portalReceivedId)
        {
            try
            {
                var portalReceivedMasterDto = new PortalReceivedMasterDto();
                var portalReceivedDetailsDtos = new List<PortalReceivedDetailsDto>();

                using (con)
                {
                    await con.OpenAsync();

                    using (SqlCommand command = new SqlCommand("GetPortalReceivedDetailsAndMaster", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PortalReceivedId", portalReceivedId);


                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var portalReceivedDetailsDto = new PortalReceivedDetailsDto
                                {
                                    PortalReceivedId = reader.GetInt32(reader.GetOrdinal("PortalReceivedId")),
                                    ProductGroupId = reader.GetInt32(reader.GetOrdinal("ProductGroupId")),
                                    ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                    UnitId = reader.GetInt32(reader.GetOrdinal("UnitId")),
                                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                                    Remarks = reader.GetOrdinal("Remarks").ToString(),
                                    Specification = reader.GetOrdinal("Specification").ToString(),
                                    Price = reader.GetOrdinal("Price"),
                                    TotalPrice = reader.GetOrdinal("TotalPrice"),


                                };
                                portalReceivedDetailsDtos.Add(portalReceivedDetailsDto);
                            }

                            if (await reader.NextResultAsync())
                            {
                                if (await reader.ReadAsync())
                                {
                                    portalReceivedMasterDto.PortalReceivedId = reader.GetInt32(reader.GetOrdinal("PortalReceivedId"));
                                    portalReceivedMasterDto.PortalReceivedCode = reader.GetString(reader.GetOrdinal("PortalReceivedCode"));
                                    portalReceivedMasterDto.ChallanNo = reader.GetOrdinal("ChallanNo").ToString();
                                    //  portalReceivedMasterDto.MaterialReceivedDate = reader.IsDBNull(reader.GetOrdinal("MaterialReceivedDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("MaterialReceivedDate"));


                                    portalReceivedMasterDto.MaterialReceivedDate = reader.GetDateTime(reader.GetOrdinal("MaterialReceivedDate"));
                                    portalReceivedMasterDto.Remarks = reader.GetOrdinal("Remarks").ToString();
                                    portalReceivedMasterDto.CompanyCode = reader.GetOrdinal("CompanyCode").ToString();

                                }
                            }
                        }
                    }
                }

                portalReceivedMasterDto.PortalReceivedDetailslist = portalReceivedDetailsDtos;

                return Ok(portalReceivedMasterDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


         
            [HttpGet("GetPortalReceivedByUserId")]
            public async Task<ActionResult> GetPortalReceivedByUserId(int userId)
            {
                try
                {
                    List<PortalReceived> portalReceivedList = new List<PortalReceived>();

                    using ( con)
                    {
                        using (var command = new SqlCommand("SELECT [PortalReceivedId], [PortalReceivedCode], [MaterialReceivedDate], UserId FROM [PortalReceivedMaster] WHERE UserId = @UserId ORDER BY [PortalReceivedCode] DESC ", con))
                        {
                            command.Parameters.AddWithValue("@UserId", userId);
                            await con.OpenAsync();

                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                PortalReceived portalReceived = new PortalReceived
                                {
                                        PortalReceivedId = Convert.ToInt32(reader["PortalReceivedId"]),
                                        PortalReceivedCode = reader["PortalReceivedCode"].ToString(),
                                        MaterialReceivedDate = reader["MaterialReceivedDate"] != DBNull.Value ? Convert.ToDateTime(reader["MaterialReceivedDate"]) : (DateTime?)null,
                                        UserId = Convert.ToInt32(reader["UserId"])
                                    };
                                    portalReceivedList.Add(portalReceived);
                                }
                            }
                        }
                    }

                    if (portalReceivedList.Count == 0)
                    {   
                        return NotFound("No data found for the given user ID.");
                    }

                    return Ok(portalReceivedList);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, "An error occurred while retrieving data: " + ex.Message);
                }
            }
         

  
        [HttpGet]
        [Route("GetPortalData")]
        public async Task<IActionResult> GetPortalData(int PortalReceivedId)
        {
            using (var connection = new SqlConnection(_healthCareConnection))
            {
                try
                {
                    PortalAfterInsert portalAfterInsert = new PortalAfterInsert();
                    string portalAfter = "GetPortalDataAfterInsertByPortalReceivedId";
                    connection.Open();
                    SqlCommand cmdportal = new SqlCommand(portalAfter, connection);
                    cmdportal.CommandType = CommandType.StoredProcedure;
                    cmdportal.Parameters.AddWithValue("@PortalReceivedId", PortalReceivedId);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmdportal);
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
                    DataTable reader = ds.Tables[0];
                    DataTable reader1 = ds.Tables[1];
                    for (int i = 0; i < reader.Rows.Count; i++)
                    {
                        portalAfterInsert.PortalReceivedId = Convert.ToInt32(reader.Rows[i]["PortalReceivedId"].ToString());
                        portalAfterInsert.PortalReceivedCode = reader.Rows[i]["PortalReceivedCode"].ToString();
                        portalAfterInsert.MaterialReceivedDate = Convert.ToDateTime(reader.Rows[i]["MaterialReceivedDate"].ToString());
                        portalAfterInsert.ChallanNo = reader.Rows[i]["ChallanNo"].ToString();
                        portalAfterInsert.ChallanDate = reader.Rows[i]["ChallanDate"] != DBNull.Value ? Convert.ToDateTime(reader.Rows[i]["ChallanDate"]) : (DateTime?)null;
                        portalAfterInsert.Remarks = reader.Rows[i]["Remarks"].ToString();
                    }
                    for (int i = 0; i < reader1.Rows.Count; i++)
                    {
                        PortalReceivedDetailAfterInsert portalReceivedDetailAfterInsert = new PortalReceivedDetailAfterInsert
                        {
                            PortalReceivedId = Convert.ToInt32(reader1.Rows[i]["PortalReceivedId"].ToString()),
                            PortalDetailsId = Convert.ToInt32(reader1.Rows[i]["PortalDetailsId"].ToString()),
                            ProductGroupId = Convert.ToInt32(reader1.Rows[i]["ProductGroupId"].ToString()),
                            ProductGroupName = reader1.Rows[i]["ProductGroupName"].ToString(),
                            ProductId = Convert.ToInt32(reader1.Rows[i]["ProductId"].ToString()),
                            ProductName = reader1.Rows[i]["ProductName"].ToString(),
                            Specification = reader1.Rows[i]["Specification"].ToString(),
                            ReceivedQty = Convert.ToDecimal(reader1.Rows[i]["ReceivedQty"].ToString()),
                            UnitId = Convert.ToInt32(reader1.Rows[i]["UnitId"].ToString()),
                            Unit = reader1.Rows[i]["Unit"].ToString(),
                            Price = Convert.ToDecimal(reader1.Rows[i]["Price"].ToString()),
                            TotalPrice = Convert.ToDecimal(reader1.Rows[i]["TotalPrice"].ToString()),
                            AvailableQty = Convert.ToDecimal(reader1.Rows[i]["AvailableQty"].ToString()),
                            Remarks = reader1.Rows[i]["Remarks"].ToString(),
                        };
                        portalAfterInsert.PortalReceivedDetailAfterInsertlList.Add(portalReceivedDetailAfterInsert);
                    }
                    return Ok(new { message = "portal Data After Insert got successfully", portalAfterInsert });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { message = "An error occurred while fetching the portal Data After Insert." });
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }
        }


    }

}
