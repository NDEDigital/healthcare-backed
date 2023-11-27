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
            _connectionSteel = config.GetConnectionString("DefaultConnection");
            _prominentConnection = config.GetConnectionString("ProminentConnection");
            _connectionNimpex = config.GetConnectionString("NimpexConnection");
            _connectionDigitalMarket = config.GetConnectionString("DigitalMarketConnection");
        }




        [HttpGet ]
        [Route("GetGoodsList")]
        public List<GoodsQuantityModel> GetGoodsList()
        {
            List<GoodsQuantityModel> Lst = new List<GoodsQuantityModel>();
            SqlConnection con = new SqlConnection(_prominentConnection);
         
            string query = @"SELECT D.CompanyId, A.GroupCode, A.GoodsID, D.GroupName, A.GoodsName,
            ISNULL(A.Spec1,'') + ' ' + ISNULL(A.Spec2,'') + ' ' + ISNULL(A.Spec3,'') 
		    + ' ' + ISNULL(A.Spec4,'') AS Specification FROM GoodsDefinition A
            JOIN GoodsGroupMaster D ON A.GroupCode = D.GroupCode;";

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
                modelObj.CompanyName = "NDE Prominent";
                modelObj.GroupCode = dt.Rows[i]["GroupCode"].ToString();
                modelObj.GoodsID = dt.Rows[i]["GoodsID"].ToString();
                modelObj.GroupName = dt.Rows[i]["GroupName"].ToString();
                modelObj.GoodsName = dt.Rows[i]["GoodsName"].ToString();
                modelObj.Specification = dt.Rows[i]["Specification"].ToString();
                modelObj.ApproveSalesQty = "30" ;
                modelObj.SalesQty =  "45";
                modelObj.StockQty =  "454";
                modelObj.SellerCode = "USR-STL-MDL-23-11-0003";
                //Console.WriteLine(i);

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
                int matchExists = 0;
                connection.Open();

                string query = @"SELECT CASE WHEN EXISTS (
                            SELECT 1
                            FROM GoodsGroupMaster
                            WHERE GroupName = @groupName And GroupCode =@groupCode
                        ) THEN 1 ELSE 0 END AS MatchExists";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    
                    command.Parameters.AddWithValue("@groupName", GroupName);
                    command.Parameters.AddWithValue("@groupCode", GroupCode);

                    matchExists = (int)command.ExecuteScalar();
                    if (matchExists > 0)
                    {
                        ProductCompanyModel obj = new ProductCompanyModel();
                        obj.CompanyName = "NDE Prominent";
                        obj.CompanyCode = "1";
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
            if (CompanyCode == "1")
            {
                SqlConnection con = new SqlConnection(_prominentConnection);
                con.Open();
                string query = @"SELECT A.GroupCode, A.GoodsID, D.GroupName, A.GoodsName,
                ISNULL(A.Spec1,'') + ' ' + ISNULL(A.Spec2,'') + ' ' + ISNULL(A.Spec3,'') + ' ' + ISNULL(A.Spec4,'') AS Specification
                FROM GoodsDefinition A JOIN GoodsGroupMaster D ON A.GroupCode = D.GroupCode";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@GroupName", GroupName);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();

                adapter.Fill(dt);
                //Console.WriteLine(dt);
                con.Close();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    GoodsQuantityModel modelObj = new GoodsQuantityModel();
                    modelObj.CompanyName = "NDE Prominent";
                    modelObj.GroupCode = dt.Rows[i]["GroupCode"].ToString();
                    modelObj.GoodsID = dt.Rows[i]["GoodsID"].ToString();
                    modelObj.GroupName = dt.Rows[i]["GroupName"].ToString();
                    modelObj.GoodsName = dt.Rows[i]["GoodsName"].ToString();
                    modelObj.Specification = dt.Rows[i]["Specification"].ToString();
                    modelObj.ApproveSalesQty = "30";
                    modelObj.SalesQty = "45";
                    modelObj.StockQty = "454";
                    modelObj.SellerCode = "USR-STL-MDL-23-11-0003";
                    //Console.WriteLine(i);

                    res.Add(modelObj);
                }
            }
          
     
            return res;
        }



    }
}
