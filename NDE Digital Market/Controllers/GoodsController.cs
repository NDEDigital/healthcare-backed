﻿using NDE_Digital_Market.Model;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.Web;

namespace NDE_Digital_Market.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoodsController : ControllerBase
    {
      
        private readonly string _prominentConnection;
        private readonly string _connectionDigitalMarket;
        private readonly string _healthCareConnection;
        public GoodsController(IConfiguration config)
        {
            _prominentConnection = config.GetConnectionString("ProminentConnection");
            _connectionDigitalMarket = config.GetConnectionString("DigitalMarketConnection");
            _healthCareConnection = config.GetConnectionString("HealthCare");
        }

        // ============ NavData ============================

        [HttpGet]
        [Route("GetNavData")]
        public async Task<List<NavModel>> GetNavData()
        {
            List<NavModel> lst = new List<NavModel>();
            using (SqlConnection con = new SqlConnection(_healthCareConnection))
            {
                await con.OpenAsync();
                string query = @"SELECT * FROM ProductGroups Where IsActive = 1";

                using (SqlCommand cmd = new SqlCommand(query, con))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        NavModel modelObj = new NavModel
                        {
                            ProductGroupCode = reader["ProductGroupCode"].ToString(),
                            ProductGroupName = reader["ProductGroupName"].ToString(),
                            ProductGroupPrefix = reader["ProductGroupPrefix"].ToString(),
                            ProductGroupDetails = reader["ProductGroupDetails"].ToString(),
                            ProductGroupID = Convert.ToInt32(reader["ProductGroupID"])
                        };
                        lst.Add(modelObj);
                    }
                }

            }


            return lst;
        }

        //================================== Get Slider GoodsList ================

        [HttpGet ]
        [Route("GetGoodsList")]
        public async Task<List<GoodsQuantityModel>> GetGoodsList()
        {
            List<GoodsQuantityModel> Lst = new List<GoodsQuantityModel>();
            SqlConnection con = new SqlConnection(_prominentConnection);
         
            string query = @"SELECT 
                            ProductList.GoodsId, 
                            ProductList.GoodsName, 
                            ProductList.GroupCode,
                            ProductList.GroupName,
                            ProductList.Specification,
                            ProductList.Price,
                            ProductList.SellerCode,
                            ProductList.ImagePath,
                            ISNULL(MaterialStockQty.PresentQty,0) AS Quantity,
                            ProductList.QuantityUnit,  
	                        UserRegistration.CompanyName
                        FROM 
                            ProductList
                        LEFT JOIN 
                            UserRegistration
                        ON 
                            ProductList.SellerCode = UserRegistration.UserCode
                        LEFT JOIN
                        MaterialStockQty
                        ON 
                           MaterialStockQty.GroupCode = ProductList.GroupCode AND  MaterialStockQty.GoodsId = ProductList.GoodsId
                        WHERE 
                            ProductList.Status = 'approved';";

            SqlCommand cmd = new SqlCommand(query, con);
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            con.Open();
            adapter.Fill(dt);
            //Console.WriteLine(dt);
            con.Close();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                GoodsQuantityModel modelObj = new GoodsQuantityModel();
                modelObj.CompanyName = dt.Rows[i]["CompanyName"].ToString();
                modelObj.GroupCode = dt.Rows[i]["GroupCode"].ToString();
                modelObj.GoodsId = dt.Rows[i]["GoodsID"].ToString();
                modelObj.GroupName = dt.Rows[i]["GroupName"].ToString();
                modelObj.GoodsName = dt.Rows[i]["GoodsName"].ToString();
                modelObj.Specification = dt.Rows[i]["Specification"].ToString();
                modelObj.ApproveSalesQty = float.Parse(dt.Rows[i]["Quantity"].ToString());
                modelObj.SellerCode = dt.Rows[i]["SellerCode"].ToString();
                modelObj.Price = float.Parse(dt.Rows[i]["Price"].ToString());
                modelObj.QuantityUnit = dt.Rows[i]["QuantityUnit"].ToString();
                modelObj.ImagePath = dt.Rows[i]["ImagePath"].ToString();


                Lst.Add(modelObj);
            }

          

            return Lst;
        }

        //====================== ProductCompany =================

      
        [HttpPost]
        [Route("GetProductCompany")]
        public List<ProductCompanyModel> GetProductCompany(string GroupCode, string GroupName)
        {

          //  string GroupName = HttpUtility.UrlDecode(EncodedGroupName);
            List<ProductCompanyModel> res = new List<ProductCompanyModel>();
            using (SqlConnection connection = new SqlConnection(_prominentConnection))
            {
             
                connection.Open();

                string query = @"SELECT 
                                          MAX(UR.CompanyName) AS CompanyName,  -- Using MAX() to get one CompanyName per CompanyCode
                                          UR.CompanyCode
                                        FROM ProductList
                                        LEFT JOIN UserRegistration AS UR ON ProductList.SellerCode = UR.UserCode
                                        WHERE ProductList.GroupCode = @GroupCode
                                          AND ProductList.GroupName = @GroupName
                                          AND ProductList.Status = 'approved'
                                        GROUP BY UR.CompanyCode;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    
                    command.Parameters.AddWithValue("@GroupName", GroupName);
                    command.Parameters.AddWithValue("@GroupCode", GroupCode);
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dt = new DataTable();

                    adapter.Fill(dt);
                    connection.Close();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        ProductCompanyModel obj = new ProductCompanyModel();
                        obj.CompanyName = dt.Rows[i]["CompanyName"].ToString();
                        obj.CompanyCode = dt.Rows[i]["CompanyCode"].ToString();
                        res.Add(obj);
                    }

                    
                }        
            }

            return res;
        }

        [HttpGet]
        [Route("GetProductList")]
        public List<GoodsQuantityModel> GetProductList(string CompanyCode, string GroupName)
        {
            List<GoodsQuantityModel> res = new List<GoodsQuantityModel>();
          
                SqlConnection con = new SqlConnection(_prominentConnection);
                con.Open();
                string query = @"SELECT 
                            ProductList.GoodsId, 
                            ProductList.GoodsName, 
                            ProductList.GroupCode,
                            ProductList.GroupName,
                            ProductList.Specification,
                            ProductList.Price,
                            ProductList.SellerCode,
                            ProductList.ImagePath,
                            ISNULL(MaterialStockQty.PresentQty,0) AS Quantity,
                            ProductList.QuantityUnit,  
	                        UserRegistration.CompanyName
                        FROM 
                            ProductList
                        LEFT JOIN 
                            UserRegistration
                        ON 
                            ProductList.SellerCode = UserRegistration.UserCode
                        LEFT JOIN
                        MaterialStockQty
                        ON 
                           MaterialStockQty.GroupCode = ProductList.GroupCode AND  MaterialStockQty.GoodsId = ProductList.GoodsId
                        WHERE 
                            ProductList.Status = 'approved' AND UserRegistration.CompanyCode = @CompanyCode AND ProductList.GroupName = @GroupName";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@GroupName", GroupName);
                cmd.Parameters.AddWithValue("@CompanyCode", CompanyCode);
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();

                adapter.Fill(dt);
                //Console.WriteLine(dt);
                con.Close();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    GoodsQuantityModel modelObj = new GoodsQuantityModel();
                    modelObj.CompanyName = dt.Rows[i]["CompanyName"].ToString();
                    modelObj.GroupCode = dt.Rows[i]["GroupCode"].ToString();
                    modelObj.GoodsId = dt.Rows[i]["GoodsId"].ToString();
                    modelObj.GroupName = dt.Rows[i]["GroupName"].ToString();
                    modelObj.GoodsName = dt.Rows[i]["GoodsName"].ToString();
                    modelObj.Specification = dt.Rows[i]["Specification"].ToString();
                    modelObj.ApproveSalesQty = float.Parse(dt.Rows[i]["Quantity"].ToString());
                    modelObj.SellerCode = dt.Rows[i]["SellerCode"].ToString();
                    modelObj.Price = float.Parse(dt.Rows[i]["Price"].ToString());
                    modelObj.QuantityUnit = dt.Rows[i]["QuantityUnit"].ToString();
                    modelObj.ImagePath = dt.Rows[i]["ImagePath"].ToString();

                    res.Add(modelObj);
                }
            
          
     
            return res;
        }


        //========================   GetBank Data =================

        [HttpGet]
        [Route("BankData")]
        public async Task<IActionResult> GetBankData()
        {
            try
            {
                var banks = new List<BankModel>();
                using (var con = new SqlConnection(_healthCareConnection))
                {
                    await con.OpenAsync();
                    string query = @"SELECT BankId, BankName FROM Banks WHERE IsActive = 1"; // Use 1 for bit field true

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var bank = new BankModel
                            {
                                BankId = reader.GetInt32(reader.GetOrdinal("BankId")),
                                BankName = reader.GetString(reader.GetOrdinal("BankName"))
                            };
                            banks.Add(bank);
                        }
                    }
                }

                if (banks.Count == 0)
                {
                    return NotFound("No active banks found.");
                }

                return Ok(banks);
            }
            catch (Exception ex)
            {
                // Consider logging the exception details here
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        //========================   GetBank Data =================
        [HttpGet]
        [Route("MobileBankData")]
        public async Task<IActionResult> GetMobileBankData()
        {
            try
            {
                var mobileBanks = new List<MobileBankModel>();
                using (var con = new SqlConnection(_healthCareConnection))
                {
                    await con.OpenAsync();
                    string query = @"SELECT * FROM MobileBankingType WHERE IsActive = 1"; 

                    using (var cmd = new SqlCommand(query, con))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var bank = new MobileBankModel
                            {
                                MobileBankingTypeId = reader.GetInt32(reader.GetOrdinal("MobileBankingTypeId")),
                                MobileBankingTypeName = reader.GetString(reader.GetOrdinal("MobileBankingType")) 
                            };
                            mobileBanks.Add(bank);
                        }
                    }
                }

                if (mobileBanks.Count == 0)
                {
                    return NotFound("No mobile banks found.");
                }

                return Ok(mobileBanks);
            }
            catch (Exception ex)
            {
                // Log the exception here
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        //========================  GetPaymentMethodType =================
        [HttpGet]
        [Route("PaymentMethodType")]
        public async Task<IActionResult> GetPaymentMethodType()
        {
            try
            {
                var paymentMethod = new List<PaymentMethodType>();
                using (var con = new SqlConnection(_healthCareConnection))
                {
                    await con.OpenAsync();
                    string query = @"SELECT * FROM PaymentMethodType WHERE IsActive = 1";

                    using (var cmd = new SqlCommand(query, con))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var bank = new PaymentMethodType
                            {
                                PaymentMethodId = reader.GetInt32(reader.GetOrdinal("PaymentMethodId")),
                                PaymentMethod = reader.GetString(reader.GetOrdinal("PaymentMethod"))
                            };
                            paymentMethod.Add(bank);
                        }
                    }
                }

                if (paymentMethod.Count == 0)
                {
                    return NotFound("No Payment Method found.");
                }

                return Ok(paymentMethod);
            }
            catch (Exception ex)
            {
                // Log the exception here
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }







    }
}
