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
        //private readonly string foldername = "D:/HealthCare/healthcare-frontend/src/assets/images/Productfiles";
        private readonly string filename = "Productfile";
        public ProductListController(IConfiguration configuration)
        {
            _configuration = configuration;
            con = new SqlConnection(_configuration.GetConnectionString("HealthCare"));
            CommonServices commonServices = new CommonServices(_configuration);
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

        [HttpPost("CreateProductList")]
        public async Task<IActionResult> CreateProductGroupsAsync([FromForm] ProductListDto productListDto)
        {
            try
            {
                Boolean check = await ProductNameCheck(productListDto.ProductName, productListDto.Specification);
                if (check)
                {
                    return BadRequest(new { message = "ProductName and Specification is same!" });
                    //return Ok("ProductName and Specification is same.");
                }
                else
                {
                    string systemCode = string.Empty;

                    // Execute the stored procedure to generate the system code
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

                    int ProductID = int.Parse(systemCode.Split('%')[0]);
                    //string ProductGroupsCode = systemCode.Split('%')[1];

                    //SP END
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

        [HttpGet]
        [Route("GetProductList")]
        public async Task<IActionResult> GetProductGroupsListAsync()
        {
            try
            {
                List<GetProductListDto> lst = new List<GetProductListDto>();
                await con.OpenAsync();
                string query = "select ProductId, ProductName from ProductList where IsActive = 1 ORDER BY ProductId DESC;";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            GetProductListDto modelObj = new GetProductListDto();
                            modelObj.ProductId = Convert.ToInt32(reader["ProductId"]);
                            modelObj.ProductName = reader["ProductName"].ToString();

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
                    query = @"SELECT * FROM ProductList WHERE IsActive= @IsActive ORDER BY ProductId  DESC;";

                }
                else
                    query = @"SELECT * FROM ProductList WHERE CONVERT(DATE, AddedDate) = CONVERT(DATE, GETDATE()) ORDER BY ProductId  DESC";
                {

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
                            modelObj.ProductGroupID = Convert.ToInt32(reader["ProductGroupID"]);
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
    }
}
