using NDE_Digital_Market.Model;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace NDE_Digital_Market.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoodsController : ControllerBase
    {
        private readonly string _connectionSteel;
        private readonly string _connectionNimpex;
        private readonly string _connectionDigitalMarket;
        public GoodsController(IConfiguration config)
        {
            _connectionSteel = config.GetConnectionString("DefaultConnection");
            _connectionNimpex = config.GetConnectionString("NimpexConnection");
            _connectionDigitalMarket = config.GetConnectionString("DigitalMarketConnection");
        }




        [HttpGet ]
        [Route("GetGoodsList")]
        public List<GoodsQuantityModel> GetGoodsList()
        {
            List<GoodsQuantityModel> Lst = new List<GoodsQuantityModel>();
            SqlConnection con = new SqlConnection(_connectionSteel);
            con.Open();
                 string query = @"
                SELECT D.CompanyId, A.GroupCode, A.GoodsID, D.GroupName, A.GoodsName,
                ISNULL(A.Spec1,'') + ' ' + ISNULL(A.Spec2,'') + ' ' + ISNULL(A.Spec3,'') + ' ' + ISNULL(A.Spec4,'') AS Specification,
                ISNULL(B.ApproveSalesQty, 0) AS ApproveSalesQty,
                ISNULL(C.SalesOrderQty, 0) AS SalesQty,
                ISNULL(B.ApproveSalesQty, 0) - ISNULL(C.SalesOrderQty, 0) AS StockQty
            FROM GoodsDefinition A
            JOIN GoodsGroupMaster D ON A.GroupCode = D.GroupCode
            LEFT JOIN (
                SELECT GroupCode, GoodsID, SUM(ApprovedQty) AS ApproveSalesQty
                FROM MaterialStockForSales
                GROUP BY GroupCode, GoodsID
            ) B ON A.GroupCode = B.GroupCode AND A.GoodsID = B.GoodsID
            LEFT JOIN (
                SELECT GoodsDefinitionId, SUM(SalesOrderQty) AS SalesOrderQty
                FROM SalesOrderDetails
                GROUP BY GoodsDefinitionId
            ) C ON A.GoodsDefinitionId = C.GoodsDefinitionId
            WHERE A.GroupCode IN ('GG-17-0086', 'GG-17-0115', 'GG-17-0035', 'GG-17-0127')
            ORDER BY A.GroupCode, ApproveSalesQty DESC;";

            SqlCommand cmd = new SqlCommand(query, con);
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            
            adapter.Fill(dt);
            //Console.WriteLine(dt);
            con.Close();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                GoodsQuantityModel modelObj = new GoodsQuantityModel();
                modelObj.CompanyName = "NDE Steel Structure";
                modelObj.GroupCode = dt.Rows[i]["GroupCode"].ToString();
                modelObj.GoodsID = dt.Rows[i]["GoodsID"].ToString();
                modelObj.GroupName = dt.Rows[i]["GroupName"].ToString();
                modelObj.GoodsName = dt.Rows[i]["GoodsName"].ToString();
                modelObj.Specification = dt.Rows[i]["Specification"].ToString();
                modelObj.ApproveSalesQty = dt.Rows[i]["ApproveSalesQty"].ToString();
                modelObj.SalesQty = dt.Rows[i]["SalesQty"].ToString();
                modelObj.StockQty = dt.Rows[i]["StockQty"].ToString();
                modelObj.SellerCode = "USR-STL-DGTL-23-08-0017";
                //Console.WriteLine(i);

                Lst.Add(modelObj);
            }

            //------------Nimpex Start ------------

            SqlConnection NimpexConnection = new SqlConnection(_connectionNimpex);
            

            using (SqlCommand NimpexCommand = new SqlCommand("MaterialStockForAll", NimpexConnection))
            {
                NimpexConnection.Open();
                NimpexCommand.CommandType = CommandType.StoredProcedure;
              
                DataTable dataTable = new DataTable();
                SqlDataAdapter NimpexCommandAdapter = new SqlDataAdapter(NimpexCommand);
                NimpexCommandAdapter.Fill(dataTable);
                NimpexConnection.Close();
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    GoodsQuantityModel NimpexObj = new GoodsQuantityModel();
                    NimpexObj.CompanyName = "Nimpex Solution";
                    NimpexObj.GroupCode = dataTable.Rows[i]["GroupCode"].ToString();
                    NimpexObj.GoodsID = dataTable.Rows[i]["GoodsID"].ToString();
                    NimpexObj.GroupName = dataTable.Rows[i]["GroupName"].ToString();
                    NimpexObj.GoodsName = dataTable.Rows[i]["GoodsName"].ToString();
                    NimpexObj.Specification = dataTable.Rows[i]["Specification"].ToString();
                    NimpexObj.StockQty = dataTable.Rows[i]["StockQty"].ToString();
                    NimpexObj.Price = "1200";
                    NimpexObj.SellerCode = "USR-STL-DGTL-23-09-0029";
                    Lst.Add(NimpexObj);
                }


            }
               //------------------------NDE_Digital_Market ------------------ 

            string sellerQuery = @"SELECT
                                    ur.CompanyName AS CompanyName,
                                    pl.MaterialType AS GroupCode,
                                    pl.ProductId AS GroupId,
                                    pl.ProductId As GoodsId,
                                    pl.MaterialName AS GroupName,
                                    pl.ProductName AS GoodsName,
                                    pl.ProductDescription AS Specification,
                                    ISNULL(msq.PresentQty, pl.Quantity) AS ApprovedSalesQuantity,
                                    pl.Quantity AS StockQty,
                                    pl.Price,
                                    ur.UserCode AS SellerCode
                                FROM
                                    [ProductList] AS pl
                                INNER JOIN
                                    [UserRegistration] AS ur ON pl.SupplierCode = ur.UserCode
                                LEFT JOIN
                                    [MaterialStockQty] AS msq ON pl.ProductId = msq.GoodsId 
                                    AND pl.MaterialType = msq.GroupCode 
                                    AND ur.UserCode = msq.SellerCode";
            con.Open();
            SqlConnection digitalCon = new SqlConnection(_connectionDigitalMarket);
            SqlCommand command = new SqlCommand(sellerQuery, digitalCon);
            SqlDataAdapter adapterSeller = new SqlDataAdapter(command);
            DataTable dtbl = new DataTable();

            adapterSeller.Fill(dtbl);
            //Console.WriteLine(dt);
            con.Close();
            for (int i = 0; i < dtbl.Rows.Count; i++)
            {
                GoodsQuantityModel modelObj = new GoodsQuantityModel();
                modelObj.CompanyName = dtbl.Rows[i]["CompanyName"].ToString();
                modelObj.GroupCode = dtbl.Rows[i]["GroupCode"].ToString();
                modelObj.GoodsID = dtbl.Rows[i]["GoodsId"].ToString();
                modelObj.GroupName = dtbl.Rows[i]["GroupName"].ToString();
                modelObj.GoodsName = dtbl.Rows[i]["GoodsName"].ToString();
                modelObj.Specification = dtbl.Rows[i]["Specification"].ToString();
                modelObj.ApproveSalesQty = dtbl.Rows[i]["ApprovedSalesQuantity"].ToString();
                modelObj.StockQty = dtbl.Rows[i]["StockQty"].ToString();
                modelObj.Price = dtbl.Rows[i]["Price"].ToString();
                modelObj.SellerCode = dtbl.Rows[i]["SellerCode"].ToString();

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
            SqlConnection con = new SqlConnection(_connectionSteel);
            con.Open();
            string query = @"SELECT DISTINCT groupName, groupCode FROM GoodsGroupMaster
                          WHERE groupCode IN ('GG-17-0086', 'GG-17-0115', 'GG-17-0035', 'GG-17-0127');";

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


            //------------Nimpex Start ------------

            SqlConnection NimpexConnection = new SqlConnection(_connectionNimpex);


            using (SqlCommand NimpexCommand = new SqlCommand("SELECT DISTINCT groupName, groupCode FROM GoodsGroupMaster", 
                NimpexConnection))
            {
                NimpexConnection.Open();
          

                DataTable dataTable = new DataTable();
                SqlDataAdapter NimpexCommandAdapter = new SqlDataAdapter(NimpexCommand);
                NimpexCommandAdapter.Fill(dataTable);
                NimpexConnection.Close();
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    NavModel NimpexObj = new NavModel();                  
                    NimpexObj.GroupCode = dataTable.Rows[i]["GroupCode"].ToString();      
                    NimpexObj.GroupName = dataTable.Rows[i]["GroupName"].ToString();
                    Lst.Add(NimpexObj);
                }

            }

            return Lst;
        }

        [HttpPost]
        [Route("GetProductCompany")]
        public List<ProductCompanyModel> GetProductCompany(string GroupCode, string GroupName)
        {
            List<ProductCompanyModel> res = new List<ProductCompanyModel>();
            using (SqlConnection connection = new SqlConnection(_connectionSteel))
            {
                int matchExists = 0;
                connection.Open();

                string query = @"SELECT CASE WHEN EXISTS (
                            SELECT 1
                            FROM GoodsGroupMaster
                            WHERE GroupName = @groupName
                        ) THEN 1 ELSE 0 END AS MatchExists";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    
                    command.Parameters.AddWithValue("@groupName", GroupName);

                    matchExists = (int)command.ExecuteScalar();
                    if (matchExists > 0)
                    {
                        ProductCompanyModel obj = new ProductCompanyModel();
                        obj.CompanyName = "NDE Steed Structure";
                        obj.CompanyCode = "1";
                        res.Add(obj);
                    }
                }

                string sellerCompany = @"   SELECT DISTINCT ur.CompanyName, pl.SupplierCode
                                            FROM   [DigitalMarket_NDE].[dbo].[ProductList] pl
                                            JOIN   [DigitalMarket_NDE].[dbo].[UserRegistration] ur ON pl.SupplierCode = ur.UserCode
                                            WHERE pl.MaterialName = @groupName AND pl.Status = 'approved';";
                using (SqlCommand command = new SqlCommand(sellerCompany, connection))
                {
                    command.Parameters.AddWithValue("@groupName", GroupName); 
                    //command.Parameters.AddWithValue("@groupName", GroupName);
                    using SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string CompanyName = reader["CompanyName"].ToString();
                        string SupplierCode = reader["SupplierCode"].ToString();
                        ProductCompanyModel obj = new ProductCompanyModel();
                        obj.CompanyName = CompanyName;
                        obj.CompanyCode = SupplierCode;
                        res.Add(obj);
                    }
                }



            }

            using (SqlConnection connection = new SqlConnection(_connectionNimpex))
            {
                int matchExists = 0;
                connection.Open();

                string query = @"SELECT CASE WHEN EXISTS (
                            SELECT 1
                            FROM GoodsGroupMaster
                            WHERE GroupName = @groupName
                        ) THEN 1 ELSE 0 END AS MatchExists";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                   
                    command.Parameters.AddWithValue("@groupName", GroupName);

                    matchExists = (int)command.ExecuteScalar();
                    if (matchExists > 0)
                    {
                        ProductCompanyModel obj = new ProductCompanyModel();
                        obj.CompanyName = "Nimplex Solution";
                        obj.CompanyCode = "2";
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
                SqlConnection con = new SqlConnection(_connectionSteel);
                con.Open();
                string query = @"
                SELECT A.GroupCode, A.GoodsID, D.GroupName, A.GoodsName,
                   ISNULL(A.Spec1,'') + ' ' + ISNULL(A.Spec2,'') + ' ' + ISNULL(A.Spec3,'') + ' ' + ISNULL(A.Spec4,'') AS Specification,
                    ISNULL(B.ApproveSalesQty, 0) AS ApproveSalesQty,
                    ISNULL(C.SalesOrderQty, 0) AS SalesQty,
                    ISNULL(B.ApproveSalesQty, 0) - ISNULL(C.SalesOrderQty, 0) AS StockQty
                FROM GoodsDefinition A
                JOIN GoodsGroupMaster D ON A.GroupCode = D.GroupCode
                LEFT JOIN (
                    SELECT GroupCode, GoodsID, SUM(ApprovedQty) AS ApproveSalesQty
                    FROM MaterialStockForSales
                    GROUP BY GroupCode, GoodsID
                ) B ON A.GroupCode = B.GroupCode AND A.GoodsID = B.GoodsID
                LEFT JOIN (
                    SELECT GoodsDefinitionId, SUM(SalesOrderQty) AS SalesOrderQty
                    FROM SalesOrderDetails
                    GROUP BY GoodsDefinitionId
                ) C ON A.GoodsDefinitionId = C.GoodsDefinitionId
                WHERE D.GroupName = @GroupName ORDER BY GroupCode, ApproveSalesQty DESC";

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
                    modelObj.CompanyName = "NDE Steel Structure";
                    modelObj.GroupCode = dt.Rows[i]["GroupCode"].ToString();
                    modelObj.GoodsID = dt.Rows[i]["GoodsID"].ToString();
                    modelObj.GroupName = dt.Rows[i]["GroupName"].ToString();
                    modelObj.GoodsName = dt.Rows[i]["GoodsName"].ToString();
                    modelObj.Specification = dt.Rows[i]["Specification"].ToString();
                    modelObj.ApproveSalesQty = dt.Rows[i]["ApproveSalesQty"].ToString();
                    modelObj.SalesQty = dt.Rows[i]["SalesQty"].ToString();
                    modelObj.StockQty = dt.Rows[i]["StockQty"].ToString();
                    modelObj.SellerCode = "USR-STL-DGTL-23-09-0028";
                    //Console.WriteLine(i);

                    res.Add(modelObj);
                }
            }
            else if (CompanyCode == "2")
            {
                SqlConnection conNimpex = new SqlConnection(_connectionNimpex);
                SqlCommand cmdSP = new SqlCommand("ParamiterizedMaterialStock", conNimpex);
                cmdSP.CommandType = CommandType.StoredProcedure;
                cmdSP.Parameters.AddWithValue("@GroupName", GroupName); // Set the parameter value here

                SqlDataAdapter adapter = new SqlDataAdapter(cmdSP);
                DataTable dt = new DataTable();

                conNimpex.Open();
                adapter.Fill(dt);
                conNimpex.Close();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    GoodsQuantityModel modelObj = new GoodsQuantityModel();
                    modelObj.CompanyName = "Nimpex Solution";
                    modelObj.GroupCode = dt.Rows[i]["GroupCode"].ToString();
                    modelObj.GoodsID = dt.Rows[i]["GoodsID"].ToString();
                    modelObj.GroupName = dt.Rows[i]["GroupName"].ToString();
                    modelObj.GoodsName = dt.Rows[i]["GoodsName"].ToString();
                    modelObj.Specification = dt.Rows[i]["Specification"].ToString();
                    modelObj.StockQty = dt.Rows[i]["StockQty"].ToString();
                    modelObj.ApproveSalesQty = dt.Rows[i]["StockQty"].ToString();
                    modelObj.SellerCode = "USR-STL-DGTL-23-09-0029";

                    res.Add(modelObj);
                }


            }
            else
            {
                SqlConnection con = new SqlConnection(_connectionSteel);
                con.Open();
                string query = @"  SELECT PL.MaterialType AS GroupCode,
                                   PL.ProductId AS GoodsID,
                                   PL.MaterialName AS GroupName,
                                   PL.ProductName AS GoodsName,
                                   PL.ProductDescription AS Specification,
								  CASE 
                                  WHEN MSQ.PresentQty IS NOT NULL THEN MSQ.PresentQty 
                                  ELSE PL.Quantity
                                  END AS ApproveSalesQty,
								   PL.Weight,
								   PL.Width,
								   PL.Length,
								   PL.Height,
								   PL.Finish,
								   PL.Grade,
								   PL.QuantityUnit,
								   PL.DimensionUnit,
								   PL.WeightUnit,
								   PL.ImagePath,
								   PL.Price,
                                   0 AS SalesQty,
                                   PL.Quantity AS StockQty,
	                               UR.CompanyName AS CompanyName
							FROM   
								[DigitalMarket_NDE].[dbo].[ProductList] AS PL
							JOIN   
								[DigitalMarket_NDE].[dbo].[UserRegistration] AS UR ON PL.SupplierCode = UR.UserCode  
							LEFT JOIN 
								[DigitalMarket_NDE].[dbo].[MaterialStockQty] AS MSQ ON PL.ProductId = MSQ.GoodsId AND PL.MaterialType = MSQ.GroupCode AND UR.UserCode = MSQ.SellerCode
							WHERE 
								PL.SupplierCode = 'USR-23-11-0003'
								AND PL.MaterialName = 'Checkered Plate' 
								AND PL.Status = 'approved'";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CompanyCode", CompanyCode);
                cmd.Parameters.AddWithValue("@GroupName", GroupName);
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
                    modelObj.ApproveSalesQty = dt.Rows[i]["ApproveSalesQty"].ToString();
                    modelObj.SalesQty = dt.Rows[i]["SalesQty"].ToString();
                    modelObj.StockQty = dt.Rows[i]["StockQty"].ToString();
                    modelObj.SellerCode = CompanyCode;
                    modelObj.Weight = dt.Rows[i]["Weight"].ToString();
                    modelObj.Length = dt.Rows[i]["Length"].ToString();
                    modelObj.Finish = dt.Rows[i]["Finish"].ToString();
                    modelObj.Grade = dt.Rows[i]["Grade"].ToString();
                    modelObj.QuantityUnit = dt.Rows[i]["QuantityUnit"].ToString();
                    modelObj.DimensionUnit = dt.Rows[i]["DimensionUnit"].ToString();
                    modelObj.WeightUnit = dt.Rows[i]["WeightUnit"].ToString();
                    modelObj.ImagePath = dt.Rows[i]["ImagePath"].ToString();
                    modelObj.Price = dt.Rows[i]["Price"].ToString();


                    res.Add(modelObj);
                }


            }

            return res;
        }



    }
}
