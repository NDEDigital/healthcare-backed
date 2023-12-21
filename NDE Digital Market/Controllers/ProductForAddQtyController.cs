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
        private CommonServices _commonServices;
        private readonly string _healthCareConnection;
        private readonly IConfiguration configuration;
        private readonly SqlConnection con;

        public ProductQuantityController(IConfiguration config)
        {
            _commonServices = new CommonServices(config);
            _healthCareConnection = config.GetConnectionString("HealthCare");
            configuration = config;
            con = new SqlConnection(configuration.GetConnectionString("HealthCare"));
        }


        [HttpGet("GetProductForAddQtyByUserId/{companyCode}")]
        public async Task<IActionResult> GetProductForAddQtyByUserId(string companyCode)
        {
            //string DecryptId = CommonServices.DecryptPassword(companyCode);
            var products = new List<SellerPoductListModel>();

            try
            {
                using (var connection = new SqlConnection(_healthCareConnection))
                {
                    using (var command = new SqlCommand("GetProductForAddQtyByCompanyCode", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@CompanyCode", companyCode));

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
                                    UnitId = reader.IsDBNull(reader.GetOrdinal("UnitId")) ? null : reader.GetString(reader.GetOrdinal("UnitId")),
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
                // Log the exception here
                return StatusCode(500, "An error occurred while retrieving products: " + ex.Message);
            }


            return Ok();
        }



        [HttpPost("PortalReceivedPost")]
        public async Task<IActionResult> InsertPortalReceivedAsync(PortalReceivedMasterDto portaldata)
        {
            try
            {
                string systemCode = string.Empty;

                // Execute the stored procedure to generate the system code
                SqlCommand cmdSP = new SqlCommand("spMakeSystemCode", con);
                {
                    cmdSP.CommandType = CommandType.StoredProcedure;
                    cmdSP.Parameters.AddWithValue("@TableName", "PortalReceivedMaster");
                    cmdSP.Parameters.AddWithValue("@Date", DateTime.Now.ToString("yyyy-MM-dd"));
                    cmdSP.Parameters.AddWithValue("@AddNumber", 1);

                    await con.OpenAsync();
                    var tempSystem = await cmdSP.ExecuteScalarAsync();
                    systemCode = tempSystem?.ToString() ?? string.Empty;
                    await con.CloseAsync();
                }

                int PortalReceivedId = int.Parse(systemCode.Split('%')[0]);
                string PortalReceivedCode = systemCode.Split('%')[1];
                //SP END

                SqlCommand cmd = new SqlCommand("InsertPortalReceivedMaster", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@PortalReceivedId", PortalReceivedId);
                cmd.Parameters.AddWithValue("@PortalReceivedCode", PortalReceivedCode);
                cmd.Parameters.AddWithValue("@MaterialReceivedDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@ChallanNo", portaldata.ChallanNo ?? String.Empty);

                //cmd.Parameters.AddWithValue("@ChallanNo", (object)portaldata.ChallanNo ?? DBNull.Value);
                //cmd.Parameters.AddWithValue("@ChallanNo", (object)portaldata.ChallanNo ?? DBNull.Value);


                cmd.Parameters.AddWithValue("@ChallanDate", portaldata.ChallanDate ?? (object)DBNull.Value);


                cmd.Parameters.AddWithValue("@Remarks", portaldata.Remarks ?? String.Empty);
                cmd.Parameters.AddWithValue("@UserId", portaldata.UserId);
                cmd.Parameters.AddWithValue("@CompanyCode", portaldata.CompanyCode);
 
                cmd.Parameters.AddWithValue("@AddedBy", portaldata.AddedBy);
                cmd.Parameters.AddWithValue("@AddedDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@AddedPC", portaldata.AddedPC);

                await con.OpenAsync();
                int a = await cmd.ExecuteNonQueryAsync();
                await con.CloseAsync();
                if(a > 0)
                {
                    await InsertPortalReceivedAsync(PortalReceivedId, portaldata.PortalReceivedDetailslist);
                }
                else
                {
                    return BadRequest(new { message = "Portal Master data isn't Inserted Successfully." });
                }
                return Ok(new {message = "Portal data Inserted Successfully."});
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task<IActionResult> InsertPortalReceivedAsync(int PortalReceivedId, List<PortalReceivedDetailsDto> PortalReceivedDetailsList)
        {
            try 
            {
                for (int i = 0; i < PortalReceivedDetailsList.Count; i++)
                {
                    string query = "InsertPortalReceivedDetails";
                    //checking if user already exect for not.
                    SqlCommand CheckCMD = new SqlCommand(query, con);
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
                    //cmd.Parameters.AddWithValue("@UpdatedBy", "UpdatedBy");
                    //cmd.Parameters.AddWithValue("@UpdatedDate", (object)groups.UpdatedDate ?? DBNull.Value);
                    //cmd.Parameters.AddWithValue("@UpdatedPC", "Default UpdatedPC");
                    con.Open();
                    CheckCMD.ExecuteNonQuery();
                    con.Close();



                }
            } 
            catch(Exception ex) 
            {
                return BadRequest(ex.Message);
            }

            return Ok(new { message = "Portal Details data Inserted Successfully." });
        }

    }
}
