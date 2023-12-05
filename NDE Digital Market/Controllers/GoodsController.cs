using NDE_Digital_Market.Model;
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
        private readonly string _connectionSteel;
        private readonly string _connectionNimpex;
        private readonly string _prominentConnection;
        private readonly string _connectionDigitalMarket;
        public GoodsController(IConfiguration config)
        {
      
            _prominentConnection = config.GetConnectionString("ProminentConnection");
       
            _connectionDigitalMarket = config.GetConnectionString("DigitalMarketConnection");
        }




        [HttpGet ]
        [Route("GetGoodsList")]
        public List<GoodsQuantityModel> GetGoodsList()
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
                modelObj.GoodsID = dt.Rows[i]["GoodsID"].ToString();
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


        // ============ NavData ============================

        [HttpGet]
        [Route("GetNavData")]
        public List<NavModel> GetNavData()
        {
            List<NavModel> Lst = new List<NavModel>();
            SqlConnection con = new SqlConnection(_prominentConnection);
            con.Open();
            string query = @"SELECT DISTINCT groupName, groupCode FROM GoodsGroupMaster
                          ";

            SqlCommand cmd = new SqlCommand(query, con);
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();

            adapter.Fill(dt);
            //Console.WriteLine(dt);
            con.Close();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                NavModel modelObj = new NavModel();
                modelObj.GroupCode = dt.Rows[i]["GroupCode"].ToString();    
                modelObj.GroupName = dt.Rows[i]["GroupName"].ToString();
                Lst.Add(modelObj);
            }


            

            return Lst;
        }

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
                                  UR.CompanyName,
                                  UR.CompanyCode
                                From ProductList
                                LEFT JOIN
                                UserRegistration AS UR
                                ON
                                 ProductList.SellerCode = UR.UserCode
                                 Where ProductList.GroupCode = @GroupCode AND ProductList.GroupName = @GroupName";

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
                    modelObj.GoodsID = dt.Rows[i]["GoodsID"].ToString();
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



    }
}
