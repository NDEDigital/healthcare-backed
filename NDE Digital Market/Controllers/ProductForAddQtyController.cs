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


        [HttpGet("GetProductForAddQtyByUserId/{UserId}")]
        public async Task<IActionResult> GetProductForAddQtyByUserId(int UserId)
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

                if (products.Count == 0)
                {
                    return NotFound("No products found for the given user ID.");
                }

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

                var portalReceivedMasterDto = await GetPortalReceivedDetailsAndMasterAsync(PortalReceivedId);

                return Ok(new { message = "Portal Details data Inserted Successfully.", data = portalReceivedMasterDto });
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
    }

}
