using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using NDE_Digital_Market.Model;
using NDE_Digital_Market.Model.MaterialStock;
using NDE_Digital_Market.Model.OrderModel;
using NDE_Digital_Market.SharedServices;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Metrics;


namespace NDE_Digital_Market.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : Controller
    {
        private readonly string _connectionSteel;
        private readonly string _connectionNimpex;
        private readonly string _prominentConnection;
        private readonly string _connectionDigitalMarket;
        private CommonServices _commonServices;
        public OrderController(IConfiguration config)
        {
            _commonServices = new CommonServices(config);
            _connectionSteel = config.GetConnectionString("DefaultConnection");
            _prominentConnection = config.GetConnectionString("ProminentConnection");
            _connectionDigitalMarket = config.GetConnectionString("DigitalMarketConnection");
            _connectionDigitalMarket = config.GetConnectionString("DigitalMarketConnection");
        }

        //transaction: if an exception occurs during the insertion process, the transaction is rolled back to maintain data consistency.

        //---------Added by Uthsow -----------------
        [HttpPost ]

        public IActionResult Order([FromBody] MasterDetailModel orderData)
        {
            List<MaterialStock> stocks = new List<MaterialStock>();

            try
            {
                // Your database connection logic here
                using (SqlConnection connection = new SqlConnection(_prominentConnection))
                {
                    connection.Open();

                    // Begin a transaction (optional)
                    //using (SqlTransaction transaction = connection.BeginTransaction())
                    //{
                    try
                    {
                        int masterId = 0;
                        // Insert the master data
                        foreach (var masterItem in orderData.master)
                        {
                            SqlCommand getLastMasterIdCmd = new SqlCommand("SELECT ISNULL(MAX(OrderMasterId), 0) FROM OrderMaster;", connection);

                            masterId = Convert.ToInt32(getLastMasterIdCmd.ExecuteScalar()) + 1;
                            masterItem.BuyerCode = CommonServices.DecryptPassword(masterItem.BuyerCode);

                            string systemCode = "";

                            SqlCommand cmdSP = new SqlCommand("spMakeSystemCode", connection);
                            {
                                cmdSP.CommandType = CommandType.StoredProcedure;
                                cmdSP.Parameters.AddWithValue("@TableName", "OrderMaster");
                                cmdSP.Parameters.AddWithValue("@Date", DateTime.Now.ToString("yyyy-MM-dd"));
                                cmdSP.Parameters.AddWithValue("@AddNumber", 1);
                                systemCode = cmdSP.ExecuteScalar().ToString();

                            }

                            if (!string.IsNullOrEmpty(systemCode))
                            {
                                masterItem.OrderNo = systemCode.Split('%')[1];
                            }


                            string masterInsertSql = "INSERT INTO OrderMaster (OrderMasterId, OrderNo, OrderDate," +
                                "BuyerCode, Address, PaymentMethod, NumberOfItem, TotalPrice, Status, PhoneNumber,DeliveryCharge) " +
                                "VALUES (@OrderMasterId,@OrderNo, @OrderDate, @BuyerCode, @Address, @PaymentMethod, @NumberOfItem, @TotalPrice, @Status, @PhoneNumber,@DeliveryCharge)";

                            using (SqlCommand masterCommand = new SqlCommand(masterInsertSql, connection))
                            {
                                masterCommand.Parameters.AddWithValue("@OrderMasterId", masterId);
                                masterCommand.Parameters.AddWithValue("@OrderNo", masterItem.OrderNo);
                                masterCommand.Parameters.AddWithValue("@OrderDate", DateTime.Parse(masterItem.OrderDate));
                                masterCommand.Parameters.AddWithValue("@BuyerCode", masterItem.BuyerCode);
                                masterCommand.Parameters.AddWithValue("@Address", masterItem.Address);
                                masterCommand.Parameters.AddWithValue("@PaymentMethod", masterItem.PaymentMethod);
                                masterCommand.Parameters.AddWithValue("@NumberOfItem", masterItem.NumberOfItem);
                                masterCommand.Parameters.AddWithValue("@TotalPrice", masterItem.TotalPrice);
                                masterCommand.Parameters.AddWithValue("@Status", masterItem.Status);
                                masterCommand.Parameters.AddWithValue("@PhoneNumber", masterItem.phoneNumber);
                                masterCommand.Parameters.AddWithValue("@DeliveryCharge", masterItem.DeliveryCharge);

                               
                                masterCommand.ExecuteNonQuery();
                            }
                        }




                        // Insert the detail data

                        int detailsId = 0;
                        foreach (var detailItem in orderData.detail)
                        {


                            SqlCommand getLastDetailsIdCmd = new SqlCommand("SELECT ISNULL(MAX(OrderDetailId), 0) FROM OrderDetails;", connection);

                            detailsId = Convert.ToInt32(getLastDetailsIdCmd.ExecuteScalar()) + 1;

                            string detailInsertSql = "INSERT INTO OrderDetails (OrderDetailId,OrderMasterId,GoodsId,GoodsName,Quantity, Discount, Price,DeliveryCharge,DeliveryDate,Specification, GroupCode, SellerCode, Status) " +
                                "VALUES (" + detailsId + ",@OrderMasterId, @GoodsId,@GoodsName, @Quantity, @Discount, @Price,@DeliveryCharge, @DeliveryDate, @Specification," +
                                "@GroupCode,@SellerCode,@Status)";

                            using (SqlCommand detailCommand = new SqlCommand(detailInsertSql, connection))
                            {
                                //     detailCommand.Parameters.AddWithValue("@OrderDetailId", detailsId);
                                detailCommand.Parameters.AddWithValue("@OrderMasterId", masterId);
                                detailCommand.Parameters.AddWithValue("@GoodsId", detailItem.GoodsId);
                                detailCommand.Parameters.AddWithValue("@GoodsName", detailItem.GoodsName);
                                detailCommand.Parameters.AddWithValue("@Quantity", detailItem.Quantity);
                                detailCommand.Parameters.AddWithValue("@Discount", detailItem.Discount);
                                detailCommand.Parameters.AddWithValue("@Price", detailItem.Price);
                                detailCommand.Parameters.AddWithValue("@DeliveryCharge", detailItem.DeliveryCharge);
                                detailCommand.Parameters.AddWithValue("@DeliveryDate", DateTime.Parse(detailItem.DeliveryDate));
                                detailCommand.Parameters.AddWithValue("@Specification", detailItem.Specification);
                                detailCommand.Parameters.AddWithValue("@GroupCode", detailItem.GroupCode);
                                detailCommand.Parameters.AddWithValue("@SellerCode", detailItem.SellerCode  );
                                detailCommand.Parameters.AddWithValue("@Status", detailItem.Status);
                                detailCommand.ExecuteNonQuery();

                                MaterialStock obj = new MaterialStock();
                                obj.GroupCode = detailItem.GroupCode;
                                obj.GoodsId = detailItem.GoodsId;
                                obj.SellerCode = detailItem.SellerCode;
                                obj.SalesQty = detailItem.Quantity;
                                obj.OperationType = "SUBTRACT";
                                stocks.Add(obj);
                            }
                        }

                        // Commit the transaction (if used)
                        //transaction.Commit();
                        string result = _commonServices.InsertUpdateStockQt(stocks);

                        return Ok(new { message = "Data inserted successfully." });

                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction in case of an exception
                        //transaction.Rollback();

                        // Handle the exception
                        return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });

                    }
                }
                //}
            }
            catch (Exception ex)
            {
                // Handle any exceptions here
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });

            }
        }
      

        [HttpPost, Authorize(Roles = "admin")]
       // [HttpPost ]
        [Route("GetOrderDataByDate/{pageNumber}/{pageSize}/{status}/{searchby}/{searchValue}")]
        public IActionResult GetDataByDate(int pageNumber, int pageSize, string status, string searchby, string searchValue, [FromForm] string? fromDate = null, [FromForm] string? toDate = null)
        {

            int PendingCount = 0, ApprovedCount = 0, DeliveredCount = 0, ReturnedCount = 0, CancelledCount = 0, TotalRowCount = 0 , ToReturnCount = 0;
            using SqlConnection con = new SqlConnection(_prominentConnection);
            con.Open();
            string condition = "";

            if (status != "All")
            {
                condition = " WHERE Status = @status";
            }

            if (searchValue != "All")
            {
                if (!string.IsNullOrEmpty(condition))
                {
                    condition += " AND";
                }
                else
                {
                    condition = " WHERE";
                }

                if (searchby == "OrderNo")
                {
                    condition += " OrderNo LIKE @searchValue";
                }
                else if (searchby == "TotalPrice")
                {
                    condition += " TotalPrice LIKE @searchValue";
                }
                else if (searchby == "OrderDate")
                {
                    condition += " OrderDate LIKE @searchValue";
                }
                else if (searchby == "Status")
                {
                    condition += " Status LIKE @searchValue";
                }
            }

            if (!string.IsNullOrEmpty(fromDate))
            {
                if (!string.IsNullOrEmpty(condition))
                {
                    condition += " AND";
                }
                else
                {
                    condition = " WHERE";
                }

                condition += " CONVERT(date, TRY_CONVERT(datetime, REPLACE([OrderDate], ',', ''), 101)) BETWEEN @FromDate AND @ToDate";
            }

            string query = $@"
        DECLARE @TotalRow AS INT;
        SET @TotalRow = (SELECT COUNT(*) FROM  OrderMaster);

        SELECT 
            @TotalRow AS TotalRowCount,
            (SELECT COUNT(*) FROM OrderMaster WHERE Status = 'Pending') AS PendingCount,
            (SELECT COUNT(*) FROM OrderMaster WHERE Status = 'Approved') AS ApprovedCount,
            (SELECT COUNT(*) FROM  OrderDetails WHERE Status = 'Returned') AS ReturnedCount,
    (SELECT COUNT(*) FROM  OrderDetails WHERE Status = 'to Return') AS ToReturnCount,
            (SELECT COUNT(*) FROM  OrderMaster WHERE Status = 'Cancelled') AS CancelledCount,
            (SELECT COUNT(*) FROM  OrderMaster WHERE Status = 'Delivered') AS DeliveredCount
 
        FROM  OrderMaster;

        SELECT 
            OrderMasterId, 
            OrderNo, 
            OrderDate, 
            Address, 
            PaymentMethod, 
            NumberOfItem, 
            TotalPrice, 
            Status,
            (SELECT COUNT(*) FROM  OrderMaster {condition}) AS TotalCount
        FROM  OrderMaster {condition}
        ORDER BY  OrderNo DESC
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            SqlCommand cmd = new SqlCommand(query, con);

            if (!string.IsNullOrEmpty(fromDate))
            {
                cmd.Parameters.AddWithValue("@FromDate", fromDate);
                cmd.Parameters.AddWithValue("@ToDate", toDate);
            }

            cmd.Parameters.AddWithValue("@PageSize", pageSize);
            cmd.Parameters.AddWithValue("@Offset", (pageNumber - 1) * pageSize);

            if (status != "All")
            {
                cmd.Parameters.AddWithValue("@status", status);
            }

            if (!string.IsNullOrEmpty(searchValue))
            {
                cmd.Parameters.AddWithValue("@searchValue", "%" + searchValue + "%");
            }

            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            adapter.Fill(ds);

            // Check if the dataset contains the tables you need
            if (ds.Tables.Count >= 1)
            {
                DataTable dataTable1st = ds.Tables[0]; // Get the 1st table from the dataset
                DataTable dataTable = ds.Tables[1]; // Get the 2nd table from the dataset
                foreach (DataRow row in dataTable1st.Rows)
                {
                    PendingCount = int.Parse(row["PendingCount"].ToString());
                    ApprovedCount = int.Parse(row["ApprovedCount"].ToString());
                    DeliveredCount = int.Parse(row["DeliveredCount"].ToString());
                    ReturnedCount = int.Parse(row["ReturnedCount"].ToString());
                    TotalRowCount = int.Parse(row["TotalRowCount"].ToString());
                    CancelledCount = int.Parse(row["CancelledCount"].ToString());
                    ToReturnCount = int.Parse(row["ToReturnCount"].ToString());
                    // Other status counts...
                }
                List<AdminOrderMaster> ordersData = new List<AdminOrderMaster>();
                foreach (DataRow row in dataTable.Rows)
                {
                    AdminOrderMaster modelObj = new AdminOrderMaster();
                    modelObj.OrderMasterId = row["OrderMasterId"].ToString();
                    modelObj.OrderNo = row["OrderNo"].ToString();
                    modelObj.OrderDate = row["OrderDate"].ToString();
                    modelObj.Address = row["Address"].ToString();
                    modelObj.PaymentMethod = row["PaymentMethod"].ToString();
                    modelObj.NumberOfItem = row["NumberOfItem"].ToString();
                    modelObj.TotalPrice = row["TotalPrice"].ToString();
                    modelObj.Status = row["Status"].ToString();
                    modelObj.TotalRowsCount = int.Parse(row["TotalCount"].ToString());
                    // Add other properties here...
                    ordersData.Add(modelObj);
                }
                // Create an anonymous object to hold the data in the desired format
                var result = new
                {
                    statusCount = new
                    {
                        PendingCount,
                        ApprovedCount,
                        CancelledCount,
                        ReturnedCount,
                        DeliveredCount,
                        TotalRowCount,
                        ToReturnCount
                    },
                    ordersData
                };
                return Ok(result);
            }

            return null;
        }



        [HttpPost("GetDatailsData"),Authorize(Roles = "admin")]
        public IActionResult GetDatailsData([FromForm] int OrderMasterId)
        {
            SqlConnection con = new SqlConnection(_prominentConnection);

            List<AdminOrderDetailsModel> detailsModels = new List<AdminOrderDetailsModel>();
            SqlCommand sqlCommand = new SqlCommand("SELECT [OrderMasterId],[OrderDetailId],[SellerCode] ,[GoodsId],[GoodsName],[GroupCode],[Specification],[Quantity],[Discount],[Price],[Status]" +
                            ",[DeliveryCharge],[DeliveryDate], UR.[FullName] AS SellerName,  UR.[CompanyName] AS SellerCompanyName,UR.Address AS SellerAddress,UR.[PhoneNumber] as SellerPhone FROM  OrderDetails" +
                            " LEFT JOIN UserRegistration UR ON  SellerCode = UR.[UserCode] where OrderMasterId = @OrderMasterId ", con);

            sqlCommand.CommandType = CommandType.Text;
            sqlCommand.Parameters.AddWithValue("@OrderMasterId", OrderMasterId);
            con.Open();
            SqlDataReader reader = sqlCommand.ExecuteReader();
            while (reader.Read())
            {
                AdminOrderDetailsModel details = new AdminOrderDetailsModel();
                {
                    details.CompanyName = reader["SellerCompanyName"].ToString();
                    details.GoodsName = reader["GoodsName"].ToString();
                    details.GoodsId = Convert.ToInt32(reader["GoodsId"]);
                    details.Quantity = Convert.ToInt32(reader["Quantity"]);
                    details.OrderDetailId = Convert.ToInt32(reader["OrderDetailId"]);
                    details.OrderMasterId = Convert.ToInt32(reader["OrderMasterId"]);
                    details.Price = Convert.ToSingle(reader["Price"].ToString());
                    details.Discount = Convert.ToSingle(reader["Discount"].ToString());
                    details.DeliveryCharge = Convert.ToSingle(reader["DeliveryCharge"].ToString());
                    details.Specification = reader["Specification"].ToString();
                    details.GroupCode = reader["GroupCode"].ToString();
                    details.SellerCode = reader["SellerCode"].ToString();
                    details.SellerName = reader["SellerName"].ToString();
                    details.Status = reader["Status"].ToString();
                }

                detailsModels.Add(details);
            }

            return Ok(detailsModels);
        }



        // [HttpPut("AdminOrderUpdateStatus"), Authorize(Roles = "admin")]
        [HttpPut("AdminOrderUpdateStatus")]





        public IActionResult UpdateStatus([FromForm] String? orderMasterId, [FromForm] String? detailsApprovedId, [FromForm] String? detailsCancelledId, [FromForm] string status)
        {
            string MasterIdString = "''";
            if (!string.IsNullOrEmpty(orderMasterId))
            {
                List<int> MasterIds = orderMasterId.Split(',').Select(int.Parse).ToList();
                MasterIdString = string.Join(",", MasterIds);
            }
            using SqlConnection con = new SqlConnection(_prominentConnection);
            string detailsStatus = "Cancelled";
           
            SqlCommand cmd;
      
                string CancelledString = "''";
           
                if (!string.IsNullOrEmpty(detailsCancelledId))
                {
                    List<int> CanncelledIds = detailsCancelledId.Split(',').Select(int.Parse).ToList();
                    CancelledString = string.Join(",", CanncelledIds);
                }


                cmd = new SqlCommand(" UPDATE OrderMaster SET Status = @value  WHERE OrderMasterId IN (" + MasterIdString + ") ;  UPDATE OrderDetails SET Status  = 'Pending' WHERE OrderMasterId  IN (" + MasterIdString + "); UPDATE OrderDetails SET Status = 'Rejected' WHERE OrderDetailId IN (" + CancelledString + "); ", con);
                cmd.Parameters.AddWithValue("@orderMasterId", orderMasterId);
                cmd.Parameters.AddWithValue("@value", status);
                con.Open();
                cmd.ExecuteNonQuery();
          



            return Ok(new { message = "successs!!!" });
        }
 

        [HttpPost,Authorize(Roles = "admin")]
        [Route("getReturnDataForAdmin/{pageNumber}/{pageSize}")]

        public IActionResult getReturnDataForAdmin([FromForm] string status, int pageNumber, int pageSize, [FromForm] string searchby, [FromForm] string searchValue, [FromForm] string? fromDate = null, [FromForm] string? toDate = null)
        {
            int PendingCount = 0, ApprovedCount = 0, DeliveredCount = 0, ReturnedCount = 0, CancelledCount = 0, TotalRowCount = 0, ToReturnCount = 0;
            List<ProductReturnModel> returnData = new List<ProductReturnModel>();
            using SqlConnection con = new SqlConnection(_prominentConnection);
            con.Open();
            string condition = "FROM  [ProductReturn] r  LEFT JOIN  [ReturnType] t ON r.[TypeId] = t.[TypeId]" +
                         " JOIN  OrderDetails od ON r.[DetailsId] = od.[OrderDetailId] AND od.[Status] = @status";


            if (searchValue != "All")
            {
                condition += " AND ";

                if (searchby == "OrderNo")
                {
                    condition += " r.[OrderNo] LIKE @searchValue";
                }
                else if (searchby == "GroupName")
                {
                    condition += "  r.[GroupName] LIKE @searchValue";
                }
                else if (searchby == "GoodsName")
                {
                    condition += "r.[GoodsName] LIKE @searchValue";
                }
                else if (searchby == "ReturnType")
                {
                    condition += " t.[ReturnType] LIKE @searchValue";
                }
            }

            if (!string.IsNullOrEmpty(fromDate))
            {
                

                condition += " And r.[ApplyDate] BETWEEN  @fromDate AND  @toDate";
            }

            string query = $@"
        DECLARE @TotalRow AS INT;
        SET @TotalRow = (SELECT COUNT(*) FROM  OrderMaster);

        SELECT 
            @TotalRow AS TotalRowCount,
            (SELECT COUNT(*) FROM  OrderMaster WHERE Status = 'Pending') AS PendingCount,
            (SELECT COUNT(*) FROM  OrderMaster WHERE Status = 'Approved') AS ApprovedCount,
            (SELECT COUNT(*) FROM  OrderDetails WHERE Status = 'Returned') AS ReturnedCount,
    (SELECT COUNT(*) FROM  OrderDetails WHERE Status = 'to Return') AS ToReturnCount,
            (SELECT COUNT(*) FROM  OrderMaster WHERE Status = 'Cancelled') AS CancelledCount,
            (SELECT COUNT(*) FROM  OrderMaster WHERE Status = 'Delivered') AS DeliveredCount
 
        FROM  OrderMaster;"+
                " SELECT r.[ReturnId], r.[GroupName],r.[GoodsName], r.[GroupCode], r.[GoodsId],r.[TypeId],r.[Remarks],r.[OrderNo],r.[DeliveryDate],r.[Price],r.[DetailsId],r.[SellerCode],r.[ApplyDate] ,t.[TypeId]," +
                "t.[ReturnType], od.[OrderDetailId],od.[Status] , ( SELECT COUNT(*) " + @condition + ") AS TotalRowCount " + condition + " ORDER BY OrderNo DESC" +
                " OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";


            SqlCommand cmd = new SqlCommand(query, con);

            cmd.Parameters.AddWithValue("@status", status);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);
            cmd.Parameters.AddWithValue("@Offset", (pageNumber - 1) * pageSize);
            if (!string.IsNullOrEmpty(searchValue))
            {
                cmd.Parameters.AddWithValue("@searchValue", "%" + searchValue + "%");
            }
            if (!string.IsNullOrEmpty(fromDate))
            {
                cmd.Parameters.AddWithValue("@FromDate", fromDate);
                cmd.Parameters.AddWithValue("@ToDate", toDate);
            }

            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            adapter.Fill(ds);

  
            // Check if the dataset contains the tables you need
            if (ds.Tables.Count >= 1)
            {
                DataTable dataTable1st = ds.Tables[0]; // Get the 1st table from the dataset
                DataTable dataTable = ds.Tables[1]; // Get the 2nd table from the dataset
                foreach (DataRow row in dataTable1st.Rows)
                {

                    PendingCount = int.Parse(row["PendingCount"].ToString());
                    ApprovedCount = int.Parse(row["ApprovedCount"].ToString());
                    DeliveredCount = int.Parse(row["DeliveredCount"].ToString());
                    ReturnedCount = int.Parse(row["ReturnedCount"].ToString());
                    TotalRowCount = int.Parse(row["TotalRowCount"].ToString());
                    CancelledCount = int.Parse(row["CancelledCount"].ToString());
                    ToReturnCount = int.Parse(row["ToReturnCount"].ToString());
                    // Other status counts...
                }
                List<ProductReturnModel> ordersData = new List<ProductReturnModel>();
                foreach (DataRow row in dataTable.Rows)
                {
                    ProductReturnModel modelObj = new ProductReturnModel();
                    // int
                    modelObj.TypeId = int.Parse(row["TypeId"].ToString());
                    modelObj.Price = int.Parse(row["Price"].ToString());
                    modelObj.ReturnId = int.Parse(row["ReturnId"].ToString());
                    modelObj.DetailsId = int.Parse(row["DetailsId"].ToString());
                    modelObj.totalRowsCount = int.Parse(row["TotalRowCount"].ToString());
                    // string
                    modelObj.ReturnType = row["ReturnType"].ToString();
                    modelObj.OrderNo = row["OrderNo"].ToString();
                    modelObj.GroupName = row["GroupName"].ToString();
                    modelObj.GoodsName = row["GoodsName"].ToString();
                    modelObj.ApplyDate = DateTime.Parse(row["ApplyDate"].ToString());
                    modelObj.DeliveryDate = DateTime.Parse(row["DeliveryDate"].ToString());
                    modelObj.Remarks = row["Remarks"].ToString();
                    modelObj.Status = row["Status"].ToString();

                    // Add other properties here...
                    ordersData.Add(modelObj);
                }
                // Create an anonymous object to hold the data in the desired format
                var result = new
                {
                    statusCount = new
                    {
                        PendingCount,
                        ApprovedCount,
                        CancelledCount,
                        ReturnedCount,
                        DeliveredCount,
                        TotalRowCount,
                        ToReturnCount
                    },
                    ordersData
                };
                return Ok(result);
            }

            return null;

        }


        //------------ get return data for SELLER --------

        [HttpPost, Authorize(Roles = "seller")]
        [Route("GetReturnData/{pageNumber}/{pageSize}")]
        public IActionResult getReturnData([FromForm] string status, int pageNumber, int pageSize)
        {
            List<ProductReturnModel> returnData = new List<ProductReturnModel>();
            string condition = "FROM  [ProductReturn] r " +
        "LEFT JOIN  [ReturnType] t ON r.[TypeId] = t.[TypeId]" +
        "JOIN  OrderDetails od ON r.[DetailsId] = od.[OrderDetailId] AND od.[Status] = @status";
         string sqlSelect = "SELECT r.[ReturnId],r.[GoodsName], r.[GroupName], r.[GroupCode], r.[GoodsId],r.[TypeId],r.[Remarks],r.[OrderNo],r.[DeliveryDate],r.[Price],r.[DetailsId],r.[SellerCode],r.[ApplyDate] ,t.[TypeId]," +
                "t.[ReturnType], od.[OrderDetailId],od.[Status] , ( SELECT COUNT(*) " + @condition + ") AS TotalRowCount " + condition + " ORDER BY [ApplyDate] DESC" +
                " OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            using (SqlConnection connection = new SqlConnection(_prominentConnection))
            {
                using (SqlCommand cmd = new SqlCommand(sqlSelect, connection))
                {
                    try
                    {
                        connection.Open();
                         cmd.Parameters.AddWithValue("@status", status);
                        cmd.Parameters.AddWithValue("@PageSize", pageSize);
                        cmd.Parameters.AddWithValue("@Offset", (pageNumber - 1) * pageSize);
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            ProductReturnModel returnType = new ProductReturnModel
                            {
                                TypeId = (int)reader["TypeId"],
                                ReturnType = reader["ReturnType"].ToString(),
                                Price = (double)reader["Price"],

                                Status = reader["Status"] == DBNull.Value ? null : reader["Status"].ToString(),
                                Remarks = reader["Remarks"] == DBNull.Value ? null : reader["Remarks"].ToString(),
                                GroupName = reader["GroupName"].ToString(),
                                GoodsName = reader["GoodsName"].ToString(),
                                ReturnId = (int)reader["ReturnId"],
                                DetailsId = (int)reader["DetailsId"],
                                TotalRowCount = (int)reader["TotalRowCount"],
                                ApplyDate = reader.GetDateTime(reader.GetOrdinal("ApplyDate")),
                                OrderNo = reader["OrderNo"].ToString(),
                                DeliveryDate = reader.GetDateTime(reader.GetOrdinal("DeliveryDate")),
                            };
                            returnData.Add(returnType);
                        }
                        reader.Close();
                        return Ok(returnData);
                    }
                    catch (Exception ex)
                    {
                        return BadRequest($"Error: {ex.Message}");
                    }
                }
            }
            return Ok();
        }



        //================================== Added By Rey ==============================
        //  getOrderUserInfo
        [HttpGet ]
        [Route("getOrderUserInfo")]
        public IActionResult getUserInfo(string userCode)
        {
            UserModel user = new UserModel();
            string decryptedUserCode = CommonServices.DecryptPassword(userCode);
            SqlConnection con = new SqlConnection(_prominentConnection);
            SqlCommand cmd = new SqlCommand("SELECT * FROM UserRegistration WHERE UserCode = @userCode ", con);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@userCode", decryptedUserCode);
            //Console.WriteLine(decryptedUserCode);
            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                user.FullName = reader["FullName"].ToString();
                user.PhoneNumber = reader["PhoneNumber"].ToString();
                user.Email = reader["Email"].ToString();
                user.Address = reader["Address"].ToString();
                con.Close();
                // Return the user object as a response
                return Ok(new { message = "GET single data successful", user });
            }
            else
            {
                con.Close();
                return BadRequest(new { message = "Invalid Inforamtion" });
            }
        }

        //  getOrdersForSeller
        //[HttpGet]
        //[Route("getAllOrderForSeller")]
        //public IActionResult getAllOrderForSeller(string sellerCode, int PageNumber, int PageSize, String? status = null)
        //{
        //    Console.WriteLine(sellerCode, "sellerCode");
        //    string decryptedSupplierCode = CommonServices.DecryptPassword(sellerCode);
        //    List<SellerOrderMaster> orderLst = new List<SellerOrderMaster>();
        //    List<CountsList> countsList = new List<CountsList>();
        //    // int PendingCount = 0, ProcessingCount = 0, ReadyToShipCount = 0, ShippedCount = 0, DeliveredCount = 0, CancelledCount = 0, AllCount=0;
        //    SqlConnection con = new SqlConnection(_connectionSteel);
        //    string queryForSeller = "sp_OrderMasterDataForSeller";
        //    con.Open();
        //    SqlCommand cmdForSeller = new SqlCommand(queryForSeller, con);
        //    cmdForSeller.CommandType = CommandType.StoredProcedure;
        //    cmdForSeller.Parameters.AddWithValue("@SellerCode", decryptedSupplierCode);
        //    cmdForSeller.Parameters.AddWithValue("@PageNumber", PageNumber);
        //    cmdForSeller.Parameters.AddWithValue("@PageSize", PageSize);
        //    if (status != null) { cmdForSeller.Parameters.AddWithValue("@Status", status); }
        //    SqlDataAdapter adapter = new SqlDataAdapter(cmdForSeller);
        //    DataSet ds = new DataSet();
        //    adapter.Fill(ds);
        //    DataTable dt = ds.Tables[0];
        //    DataTable dt1 = ds.Tables[1];
        //    con.Close();
        //    for (int i = 0; i < dt.Rows.Count; i++)
        //    {
        //        SellerOrderMaster order = new SellerOrderMaster();
        //        order.OrderMasterId = Convert.ToInt32(dt.Rows[i]["OrderMasterId"]);
        //        order.OrderNo = dt.Rows[i]["OrderNo"].ToString();
        //        order.OrderDate = dt.Rows[i]["OrderDate"].ToString();
        //        order.Address = dt.Rows[i]["Address"].ToString();
        //        order.Status = dt.Rows[i]["Status"].ToString();
        //        order.PaymentMethod = dt.Rows[i]["PaymentMethod"].ToString();
        //        order.NumberofItem = Convert.ToInt32(dt.Rows[i]["NumberOfItem"]);
        //        order.TotalPrice = Convert.ToDecimal(dt.Rows[i]["TotalPrice"]);
        //        orderLst.Add(order);
        //    }
        //    for (int i = 0; i < dt1.Rows.Count; i++)
        //    {
        //        CountsList counts = new CountsList();
        //        counts.PendingCount = Convert.ToInt32(dt1.Rows[i]["PendingCount"]);
        //        counts.ProcessingCount = Convert.ToInt32(dt1.Rows[i]["ProcessingCount"]);
        //        counts.ReadyToShipCount = Convert.ToInt32(dt1.Rows[i]["ReadyToShipCount"]);
        //        counts.ShippedCount = Convert.ToInt32(dt1.Rows[i]["ShippedCount"]);
        //        counts.DeliveredCount = Convert.ToInt32(dt1.Rows[i]["DeliveredCount"]);
        //        counts.CancelledCount = Convert.ToInt32(dt1.Rows[i]["CancelledCount"]);
        //        counts.AllCount = Convert.ToInt32(dt1.Rows[i]["AllCount"]);

        //        countsList.Add(counts);
        //    }
        //    return Ok(new { message = "content get successfully", orderLst, countsList });
        //}
        public class CountsList
        {
            public int PendingCount { get; set; }
            public int ProcessingCount { get; set; }
            public int ReadyToShipCount { get; set; }
            public int ShippedCount { get; set; }
            public int DeliveredCount { get; set; }
            public int CancelledCount { get; set; }
            public int AllCount { get; set; }
            public int ToReturnCount { get; set; }
            public int ReturnedCount { get; set; }



        }



        [HttpGet, Authorize(Roles = "seller")]
        [Route("getSearchedAllOrderForSeller")]
        public IActionResult getSearchedAllOrderForSeller(string sellerCode, int PageNumber, int PageSize, String? status = null, String? SearchedOrderNo = null, String? SearchedPaymentMethod = null, String? SearchedStatus = null)
        {
            Console.WriteLine(sellerCode, "sellerCode");
            string decryptedSupplierCode = CommonServices.DecryptPassword(sellerCode);
            List<SellerOrderMaster> orderLst = new List<SellerOrderMaster>();
            List<CountsList> countsList = new List<CountsList>();

            SqlConnection con = new SqlConnection(_prominentConnection);
            string queryForSeller = "sp_OrderMasterDataForSeller";
            con.Open();
            SqlCommand cmdForSeller = new SqlCommand(queryForSeller, con);
            cmdForSeller.CommandType = CommandType.StoredProcedure;
            cmdForSeller.Parameters.AddWithValue("@SellerCode", decryptedSupplierCode);
            cmdForSeller.Parameters.AddWithValue("@PageNumber", PageNumber);
            cmdForSeller.Parameters.AddWithValue("@PageSize", PageSize);
            if (status != null) { cmdForSeller.Parameters.AddWithValue("@Status", status); }
            if (SearchedOrderNo != null) { cmdForSeller.Parameters.AddWithValue("@SearchedOrderNo", SearchedOrderNo); }
            if (SearchedPaymentMethod != null) { cmdForSeller.Parameters.AddWithValue("@SearchedPaymentMethod", SearchedPaymentMethod); }
            if (SearchedStatus != null) { cmdForSeller.Parameters.AddWithValue("@SearchedStatus", SearchedStatus); }
            SqlDataAdapter adapter = new SqlDataAdapter(cmdForSeller);
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            DataTable dt = ds.Tables[0];
            DataTable dt1 = ds.Tables[1];
            con.Close();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                SellerOrderMaster order = new SellerOrderMaster();
                order.OrderMasterId = Convert.ToInt32(dt.Rows[i]["OrderMasterId"]);
                order.OrderNo = dt.Rows[i]["OrderNo"].ToString();
                order.OrderDate = dt.Rows[i]["OrderDate"].ToString() ;
                order.Address = dt.Rows[i]["Address"].ToString();
                order.Status = dt.Rows[i]["Status"].ToString();
                order.PaymentMethod = dt.Rows[i]["PaymentMethod"].ToString();
                order.NumberofItem = Convert.ToInt32(dt.Rows[i]["NumberOfItem"]);
                order.TotalPrice = Convert.ToDecimal(dt.Rows[i]["TotalPrice"]);
                order.TotalRowCount = Convert.ToInt32(dt.Rows[i]["TotalRowCount"]);
                orderLst.Add(order);
            }
            for (int i = 0; i < dt1.Rows.Count; i++)
            {
                CountsList counts = new CountsList();
                counts.PendingCount = Convert.ToInt32(dt1.Rows[i]["PendingCount"]);
                counts.ProcessingCount = Convert.ToInt32(dt1.Rows[i]["ProcessingCount"]);
                counts.ReadyToShipCount = Convert.ToInt32(dt1.Rows[i]["ReadyToShipCount"]);
                counts.ShippedCount = Convert.ToInt32(dt1.Rows[i]["ShippedCount"]);
                counts.DeliveredCount = Convert.ToInt32(dt1.Rows[i]["DeliveredCount"]);
                counts.CancelledCount = Convert.ToInt32(dt1.Rows[i]["CancelledCount"]);
                counts.AllCount = Convert.ToInt32(dt1.Rows[i]["AllCount"]);
                counts.ToReturnCount = Convert.ToInt32(dt1.Rows[i]["ToReturnCount"]);
                counts.ReturnedCount = Convert.ToInt32(dt1.Rows[i]["ReturnedCount"]);
                countsList.Add(counts);
            }
            return Ok(new { message = "content get successfully", orderLst, countsList });
        }

        // update order status for seller
        [HttpPut, Authorize(Roles = "seller")]
        [Route("updateSellerOrderStatus")]
        public IActionResult updateSellerOrderStatus([FromForm] string idList, [FromForm] string status, [FromForm] string sellerCode)
        {
            string decryptedSupplierCode = CommonServices.DecryptPassword(sellerCode);
            SqlConnection con = new SqlConnection(_prominentConnection);
            List<int> ids = idList.Split(',').Select(int.Parse).ToList();
            string idListString = string.Join(",", ids);
            SqlCommand cmd = new SqlCommand("UPDATE OrderDetails SET Status = @status WHERE OrderMasterId IN (" + idListString + ") AND SellerCode=@sellerCode  And  Status != 'Rejected'", con);
            cmd.Parameters.AddWithValue("@status", status);
            cmd.Parameters.AddWithValue("@sellerCode", decryptedSupplierCode);
            con.Open();
            cmd.ExecuteNonQuery();
            return Ok(new { message = "Sellers Order status updated successfully" });
        }

        //  getOrdersForBuyer
        [HttpGet, Authorize(Roles = "buyer")]
        [Route("getAllOrderForBuyer")]
        public IActionResult getAllOrderForBuyer(string buyerCode, int PageNumber, int rowCount, String? status = null)
        {
            string decryptedBuyerCode = CommonServices.DecryptPassword(buyerCode);
            List<BuyerOrderModel> buyerOrderLst = new List<BuyerOrderModel>();
            List<CountsList> countsList = new List<CountsList>();

            SqlConnection con = new SqlConnection(_prominentConnection);
            string queryForBuyer = "sp_OrderDataForBuyer";
            con.Open();
            SqlCommand cmdForBuyer = new SqlCommand(queryForBuyer, con);
            cmdForBuyer.CommandType = CommandType.StoredProcedure;
            cmdForBuyer.Parameters.AddWithValue("@BuyerCode", decryptedBuyerCode);
            cmdForBuyer.Parameters.AddWithValue("@PageNumber", PageNumber);
            cmdForBuyer.Parameters.AddWithValue("@rowCount", rowCount);
            cmdForBuyer.Parameters.AddWithValue("@Status", status);
            SqlDataAdapter adapter = new SqlDataAdapter(cmdForBuyer);
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            DataTable dt = ds.Tables[0];
            DataTable dt1 = ds.Tables[1];
            DataTable dt2 = ds.Tables[2];
            con.Close();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                BuyerOrderModel buyerOrder = new BuyerOrderModel();
                buyerOrder.OrderMasterId = Convert.ToInt32(dt.Rows[i]["OrderMasterId"]);
                buyerOrder.OrderNo = dt.Rows[i]["OrderNo"].ToString();
                buyerOrder.OrderDate = dt.Rows[i]["OrderDate"].ToString();
                buyerOrder.TotalPrice = Convert.ToSingle(dt.Rows[i]["TotalPrice"]);
                buyerOrder.DeliveryCharge = Convert.ToSingle(dt.Rows[i]["DeliveryCharge"]);
                buyerOrder.Subtotal = Convert.ToSingle(dt.Rows[i]["Subtotal"]);
                buyerOrder.ShippingAddress = dt.Rows[i]["ShippingAddress"].ToString();
                buyerOrder.ShippingContact = dt.Rows[i]["ShippingContact"].ToString();
                buyerOrder.BillingAddress = dt.Rows[i]["BillingAddress"].ToString();
                buyerOrder.BillingContact = dt.Rows[i]["BillingContact"].ToString();
                buyerOrder.BuyerName = dt.Rows[i]["BuyerName"].ToString();


                buyerOrderLst.Add(buyerOrder);

            }
            for (int i = 0; i < dt1.Rows.Count; i++)
            {
                OrderDetails order = new OrderDetails();
                order.OrderMasterId = Convert.ToInt32(dt1.Rows[i]["OrderMasterId"]);
                order.orderDetailId = Convert.ToInt32(dt1.Rows[i]["orderDetailId"]);
                order.GoodsId = Convert.ToInt32(dt1.Rows[i]["GoodsId"]);
                order.GoodsName = dt1.Rows[i]["GoodsName"].ToString();
                order.GroupCode = dt1.Rows[i]["GroupCode"].ToString();
                order.Quantity = Convert.ToInt32(dt1.Rows[i]["Quantity"]);
                order.Price = Convert.ToSingle(dt1.Rows[i]["Price"]);
                order.Status = dt1.Rows[i]["Status"].ToString();
                order.DeliveryDate = dt1.Rows[i]["DeliveryDate"].ToString();
                order.SellerCode = dt1.Rows[i]["SellerCode"].ToString();
                order.SellerName = dt1.Rows[i]["SellerName"].ToString();
                order.imagePath = dt1.Rows[i]["imagePath"].ToString();
                order.GroupName = dt1.Rows[i]["GroupName"].ToString();

                // Getting the BuyerOrderModel object for the corresponding order.
                BuyerOrderModel buyerOrder = buyerOrderLst.Find(bo => bo.OrderMasterId == order.OrderMasterId);
                buyerOrder.OrderDetailsList.Add(order);
            }
            int toShipCount = 0, toDeliverCount = 0, toReviewCount = 0, AllCount = 0, ToReturnCount=0, ReturnedCount=0;
            for (int i = 0; i < dt2.Rows.Count; i++)
            {
                toShipCount = Convert.ToInt32(dt2.Rows[i]["ReadyToShipCount"]);
                toDeliverCount = Convert.ToInt32(dt2.Rows[i]["ShippedCount"]);
                toReviewCount = Convert.ToInt32(dt2.Rows[i]["DeliveredCount"]);
                AllCount = Convert.ToInt32(dt2.Rows[i]["AllCount"]);
                ToReturnCount = Convert.ToInt32(dt2.Rows[i]["ToReturnCount"]);
                ReturnedCount = Convert.ToInt32(dt2.Rows[i]["ReturnedCount"]);


            }
            return Ok(new { message = "content get successfully", buyerOrderLst, toShipCount, toDeliverCount, toReviewCount, AllCount, ToReturnCount, ReturnedCount });
        }

        //checkUnderOrderProccess
        [HttpGet, Authorize(Roles = "seller")]
        [Route("checkUnderOrderProccess")]
           public IActionResult checkUnderOrderProccess( int GoodsId,string GroupCode) 
            {

            SqlConnection con = new SqlConnection(_prominentConnection);
           SqlCommand cmd = new SqlCommand("SELECT CASE   WHEN COUNT(*) > 0 THEN 'true'   ELSE 'false'" +
                "END AS isUnderOrderProccess FROM OrderDetails WHERE GoodsId = @GoodsId AND GroupCode = @GroupCode AND Status NOT IN ('Delivered', 'Reviewed', 'Cancelled');", con);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@GoodsId", GoodsId);
            cmd.Parameters.AddWithValue("@GroupCode", GroupCode);
            bool isUnderOrderProccess = false;
         con.Open();
            SqlDataReader reader=cmd.ExecuteReader();
            if (reader.Read())
            {
                string result = reader["isUnderOrderProccess"].ToString();
                if (result == "true")
                {
                    isUnderOrderProccess = true;
                }
            }
            con.Close();
                return Ok(new { message = "checked UnderOrderProccess" ,isUnderOrderProccess});
            }





        //Added by marufa for buyer

        // update order status for buyer

        [HttpPut ]
        [Route("updateDetailsOrderStatus")]
        public IActionResult updateDetailsOrderStatus([FromForm] string idList, [FromForm] string status )
        {
           
            SqlConnection con = new SqlConnection(_prominentConnection);
            List<int> ids = idList.Split(',').Select(int.Parse).ToList();
            string idListString = string.Join(",", ids);
            if(status != "Cancelled") {
                   SqlCommand cmd = new SqlCommand("UPDATE OrderDetails SET Status = @status WHERE OrderDetailId IN (" + idListString + ")  ", con);
                cmd.Parameters.AddWithValue("@status", status);
                con.Open();
                cmd.ExecuteNonQuery();

            }
        if(status == "Cancelled")
            {
                SqlCommand cmd = new SqlCommand(" Update  OrderDetails set status = 'Cancelled' where [OrderDetailId]  IN (" + idListString + ")  And status = 'Pending' ; ", con);
                con.Open();
                cmd.ExecuteNonQuery();

            }
          
          
            return Ok(new { message = "buyer Order status updated successfully" });
        }

        // ============================== Added By Rey ==========================

        [HttpGet ]
        [Route("getBuyerInfo")]
        public IActionResult getBuyerInfo(string idList)
        {
            List<int> ids = idList.Split(',').Select(int.Parse).ToList();
            string idListString = string.Join(",", ids);
            List<ShortendUserModel> users = new List<ShortendUserModel>();
            SqlConnection con = new SqlConnection(_prominentConnection);
            SqlCommand cmd = new SqlCommand(
      "SELECT OM.OrderMasterId, UR.UserId, UR.FullName, UR.Email, UR.PhoneNumber " +
      "FROM OrderMaster OM " +
      "LEFT JOIN UserRegistration UR ON OM.BuyerCode = UR.UserCode " +
      "WHERE OM.OrderMasterId IN (" + idListString + ")", con);

            cmd.CommandType = CommandType.Text;

            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                ShortendUserModel user = new ShortendUserModel();
             //   user.UserID = (int)reader["user_id"];
                user.OrderMasterId = (int)reader["OrderMasterId"];
                user.FullName = reader["FullName"].ToString();
                user.PhoneNumber = reader["PhoneNumber"].ToString();
                user.Email = reader["Email"].ToString();
                users.Add(user);
            }

            con.Close();

            if (users.Count > 0)
            {
                return Ok(new { message = "Got user data successfully", users });
            }
            else
            {
                return BadRequest(new { message = "No data found for the given IDs" });
            }
        }




        // by Marufa

        [HttpGet]
        [Route("GetSellerInventory")]
        public List<SellerInventoryModel> GetProductList(string SellerCode,String? GoodsName,String? GroupCode)
        {
            List<SellerInventoryModel> res = new List<SellerInventoryModel>();
            string decryptedSupplierCode = CommonServices.DecryptPassword(SellerCode);
         
                using (SqlConnection con = new SqlConnection(_prominentConnection))
                {
                    string query = @"SELECT ProductList.GroupCode, ProductList.GoodsId, ProductList.GoodsName,ISNULL(PresentQty,0) AS AvailableQuantity, ProductList.Quantity,ProductList.Price
                                        From ProductList
                                        LEFT JOIN 
                                        MaterialStockQty
                                        ON 
                                        ProductList.GroupCode = MaterialStockQty.GroupCode AND ProductList.GoodsId = MaterialStockQty.GoodsId AND ProductList.SellerCode = MaterialStockQty.SellerCode
                                        WHERE ProductList.SellerCode = @SellerCode AND  (
                                    (@GoodsName IS NULL OR @GoodsName = '' OR ProductList.GoodsName LIKE   @GoodsName   )
                                    AND (@GroupCode IS NULL OR @GroupCode = '' OR ProductList.GroupCode LIKE  @GroupCode   )
                                    );";
                  
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@SellerCode", decryptedSupplierCode);
                    cmd.Parameters.AddWithValue("@GoodsName", '%' + GoodsName + '%');
                    cmd.Parameters.AddWithValue("@GroupCode", '%' + GroupCode + '%');
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    con.Open();
                    adapter.Fill(dt);
                    con.Close();

                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            SellerInventoryModel modelObj = new SellerInventoryModel();

                            modelObj.GroupCode = dt.Rows[i]["GroupCode"].ToString();
                            modelObj.GoodsID = int.Parse(dt.Rows[i]["GoodsID"].ToString());
                            modelObj.GoodsName = dt.Rows[i]["GoodsName"].ToString();
                            modelObj.AvailableQuantity =Convert.ToInt32(dt.Rows[i]["AvailableQuantity"].ToString());
                            modelObj.TotalQuantity = Convert.ToInt32(dt.Rows[i]["Quantity"].ToString()); 

                            modelObj.salesQuantiy = modelObj.TotalQuantity - modelObj.AvailableQuantity;
                            modelObj.Price = Convert.ToInt32(dt.Rows[i]["Price"].ToString());

                        res.Add(modelObj);
                        }
                    }
                }
            


         

            return res;
        }


    }
}

public class ShortendUserModel
{
    public int? OrderMasterId { get; set; }
  //  public int? UserID { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
}