﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NDE_Digital_Market.Model;
using NDE_Digital_Market.DTOs;
using System.Data;
using System.Data.SqlClient;

namespace NDE_Digital_Market.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductGroupsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly SqlConnection con;
        public ProductGroupsController(IConfiguration configuration)
        {
            _configuration = configuration;
            con = new SqlConnection(_configuration.GetConnectionString("HealthCare"));
        }

        private async Task<Boolean> ProductGroupsNameCheck(string productgoodsname)
        {
            SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM  [ProductGroups] WHERE ProductGroupName = @productgoodsname", con);
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

        [HttpPost("CreateProductGroups")]
        public async Task<IActionResult> CreateProductGroupsAsync([FromForm] ProductGroupsDto productGroupsDto)
        {
            try
            {
                Boolean check = await ProductGroupsNameCheck(productGroupsDto.ProductGroupName);
                if (check)
                {
                    return Ok("ProductGroupName already exect.");
                }
                else
                {
                    string systemCode = string.Empty;

                    // Execute the stored procedure to generate the system code
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

                    //SP END
                    string query = "INSERT INTO ProductGroups (ProductGroupID, ProductGroupCode, ProductGroupName, ProductGroupPrefix, ProductGroupDetails, IsActive, AddedBy, DateAdded, AddedPC) " +
                        " VALUES(@ProductGroupID, @ProductGroupCode, @ProductGroupName, @ProductGroupPrefix, @ProductGroupDetails, @IsActive, @AddedBy, @DateAdded, @AddedPC);";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@ProductGroupID", ProductGroupsID);
                    cmd.Parameters.AddWithValue("@ProductGroupCode", ProductGroupsCode);
                    cmd.Parameters.AddWithValue("@ProductGroupName", productGroupsDto.ProductGroupName);
                    cmd.Parameters.AddWithValue("@ProductGroupPrefix", productGroupsDto.ProductGroupPrefix);
                    cmd.Parameters.AddWithValue("@ProductGroupDetails", productGroupsDto.ProductGroupDetails);
                    cmd.Parameters.AddWithValue("@IsActive", 1);
                    cmd.Parameters.AddWithValue("@AddedBy", productGroupsDto.AddedBy);
                    cmd.Parameters.AddWithValue("@DateAdded", DateTime.Now);
                    cmd.Parameters.AddWithValue("@AddedPC", productGroupsDto.AddedPC);

                    await con.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                    await con.CloseAsync();

                    return Ok("Product Group Create successfully.");
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
            string query = "SELECT [ProductGroupID],[ProductGroupCode],[ProductGroupName],[ProductGroupPrefix],[ProductGroupDetails],"+
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