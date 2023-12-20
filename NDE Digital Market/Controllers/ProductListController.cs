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
        private readonly IConfiguration _configuration;
        private readonly SqlConnection con;
        private readonly string foldername = "F:/Projects/Health Care/healthcare-frontend/src/assets/images/Productfiles";
        private readonly string filename = "Productfile";
        public ProductListController(IConfiguration configuration)
        {
            _configuration = configuration;
            con = new SqlConnection(_configuration.GetConnectionString("HealthCare"));
        }

        private async Task<Boolean> ProductNameCheck(string ProductName,string Specification)
        {
            SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM ProductList WHERE ProductName = @ProductName AND Specification = @Specification; ", con);
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

        [HttpPost("CreateProductGroups")]
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
                    string query = "INSERT INTO ProductList (ProductId, ProductName, ProductGroupID,Specification,UnitId, ImagePath, ProductSubName, IsActive, AddedDate, AddedBy, AddedPc)" +
                        "VALUES (@ProductId, @ProductName, @ProductGroupID, @Specification, @UnitId, @ImagePath, @ProductSubName, @IsActive, @AddedDate, @AddedBy, @AddedPc)";
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
        [Route("GetProductGroupsList")]
        public async Task<List<ProductGroupsModel>> GetProductGroupsListAsync()
        {
            List<ProductGroupsModel> lst = new List<ProductGroupsModel>();
            await con.OpenAsync();
            string query = "SELECT [ProductGroupID],[ProductGroupCode],[ProductGroupName],[ProductGroupPrefix],[ProductGroupDetails]," +
                " [IsActive] FROM ProductGroups WHERE IsActive = 1 ORDER BY [ProductGroupID] DESC;";

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


            return lst;
        }
    }
}
