using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using NDE_Digital_Market.Model;
using NDE_Digital_Market.Model.MaterialStock;
using NDE_Digital_Market.Model.OrderModel;
using NDE_Digital_Market.SharedServices;
using NDE_Digital_Market.DTOs;
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
        private readonly IConfiguration configuration;
        private readonly SqlConnection con;

        private readonly string _healthCareConnection;
        public OrderController(IConfiguration config)
        {
            _commonServices = new CommonServices(config);
            _connectionSteel = config.GetConnectionString("DefaultConnection");
            _prominentConnection = config.GetConnectionString("ProminentConnection");
            _connectionDigitalMarket = config.GetConnectionString("DigitalMarketConnection");
            _connectionDigitalMarket = config.GetConnectionString("DigitalMarketConnection");
            configuration = config;
            con = new SqlConnection(configuration.GetConnectionString("HealthCare"));

            _healthCareConnection = config.GetConnectionString("HealthCare");
        }

    

        [HttpPost("InsertOrderData")]
        public async Task<IActionResult> InsertOrderDateAsync(OrderMasterDto orderdata)
        {
            // Start a transaction
            SqlTransaction transaction = null;

            try
            {
                string systemCode = string.Empty;
                await con.OpenAsync();
                transaction = con.BeginTransaction();
                // Execute the stored procedure to generate the system code
                SqlCommand cmdSP = new SqlCommand("spMakeSystemCode", con, transaction);
                {
                    cmdSP.CommandType = CommandType.StoredProcedure;
                    cmdSP.Parameters.AddWithValue("@TableName", "OrderMaster");
                    cmdSP.Parameters.AddWithValue("@Date", DateTime.Now.ToString("yyyy-MM-dd"));
                    cmdSP.Parameters.AddWithValue("@AddNumber", 1);
                    var tempSystem = await cmdSP.ExecuteScalarAsync();
                    systemCode = tempSystem?.ToString() ?? string.Empty;
                }
                int OrderMasterId = int.Parse(systemCode.Split('%')[0]);
                string OrderNo = systemCode.Split('%')[1];
                // SP END

                SqlCommand cmd = new SqlCommand("InsertOrderMaster", con, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@OrderMasterId", OrderMasterId);
                cmd.Parameters.AddWithValue("@OrderNo", OrderNo);
                cmd.Parameters.AddWithValue("@OrderDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@Address", orderdata.Address);
                cmd.Parameters.AddWithValue("@UserId", orderdata.UserId);
                cmd.Parameters.AddWithValue("@PaymentMethod", orderdata.PaymentMethod ?? String.Empty);
                cmd.Parameters.AddWithValue("@NumberOfItem", orderdata.NumberOfItem);
                cmd.Parameters.AddWithValue("@TotalPrice", orderdata.TotalPrice);
                cmd.Parameters.AddWithValue("@PhoneNumber", orderdata.PhoneNumber);
                cmd.Parameters.AddWithValue("@DeliveryCharge", orderdata.DeliveryCharge);
                cmd.Parameters.AddWithValue("@Status", "Pending");

                cmd.Parameters.AddWithValue("@AddedBy", orderdata.AddedBy);
                cmd.Parameters.AddWithValue("@AddedDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@AddedPC", orderdata.AddedPC);

                int a = await cmd.ExecuteNonQueryAsync();
                if (a > 0)
                {
                    var detailsResult = await InsertOrderDateDetailsAsync(OrderMasterId, orderdata.OrderDetailsList, transaction);
                    if (detailsResult is BadRequestObjectResult)
                    {
                        throw new Exception((detailsResult as BadRequestObjectResult).Value.ToString());
                    }
                }
                else
                {
                    return BadRequest(new { message = "Order Master data isn't Inserted Successfully." });
                }
                // If everything is fine, commit the transaction
                transaction.Commit();
                return Ok(new { message = "Order data Inserted Successfully." });
            }
            catch (Exception ex)
            {
                // If there is any error, rollback the transaction
                if (transaction != null)
                {
                    transaction.Rollback();
                }
                return BadRequest(new { message = ex.Message });
            }
            finally
            {
                // Finally block to ensure the connection is always closed
                if (con.State == ConnectionState.Open)
                {
                    await con.CloseAsync();
                }
            }
        }

        private async Task<IActionResult> InsertOrderDateDetailsAsync(int OrderMasterId, List<OrderDetailsDto> OrderDetailsList, SqlTransaction transaction)
        {
            try
            {
                for (int i = 0; i < OrderDetailsList.Count; i++)
                {
                    string query = "InsertOrderDetails";
                    //checking if user already exect for not.
                    SqlCommand CheckCMD = new SqlCommand(query, con, transaction);
                    CheckCMD.CommandType = CommandType.StoredProcedure;

                    CheckCMD.Parameters.Clear();
                    CheckCMD.Parameters.AddWithValue("@OrderMasterId", OrderMasterId);
                    CheckCMD.Parameters.AddWithValue("@UserId", OrderDetailsList[i].UserId);
                    CheckCMD.Parameters.AddWithValue("@ProductId", OrderDetailsList[i].ProductId);
                    CheckCMD.Parameters.AddWithValue("@ProductGroupID", OrderDetailsList[i].ProductGroupID);
                    CheckCMD.Parameters.AddWithValue("@Specification", OrderDetailsList[i].Specification);
                    CheckCMD.Parameters.AddWithValue("@Qty", OrderDetailsList[i].Qty);
                    CheckCMD.Parameters.AddWithValue("@UnitId", OrderDetailsList[i].UnitId);
                    CheckCMD.Parameters.AddWithValue("@DiscountAmount", OrderDetailsList[i].DiscountAmount != null ? (object)OrderDetailsList[i].DiscountPct : DBNull.Value);
                    CheckCMD.Parameters.AddWithValue("@Price", OrderDetailsList[i].Price);
                    CheckCMD.Parameters.AddWithValue("@Status", "Pending");
                    CheckCMD.Parameters.AddWithValue("@DeliveryCharge", OrderDetailsList[i].DeliveryCharge);
                    CheckCMD.Parameters.AddWithValue("@DeliveryDate", OrderDetailsList[i].DeliveryDate);
                    CheckCMD.Parameters.AddWithValue("@DiscountPct",OrderDetailsList[i].DiscountPct != null ? (object)OrderDetailsList[i].DiscountPct : DBNull.Value);
                    //CheckCMD.Parameters.AddWithValue("@DiscountPct", OrderDetailsList[i].DiscountPct ?? (object)DBNull.Value);
                    CheckCMD.Parameters.AddWithValue("@NetPrice", OrderDetailsList[i].NetPrice );

                    CheckCMD.Parameters.AddWithValue("@AddedBy", OrderDetailsList[i].AddedBy);
                    CheckCMD.Parameters.AddWithValue("@AddedDate", DateTime.Now);
                    CheckCMD.Parameters.AddWithValue("@AddedPC", OrderDetailsList[i].AddedPC);


                    await CheckCMD.ExecuteNonQueryAsync();

                }
                return Ok(new { message = "Order Details data Inserted Successfully." });

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            //return Ok(new { message = "Order Details data Inserted Successfully." });
        }







        [HttpGet("GetOrderMasterData")]
        public async Task<IActionResult> GetOrderMasterData(string? status)
        {
            //string DecryptId = CommonServices.DecryptPassword(companyCode);
            var products = new List<OrderDataBaseOnStatusDto>();

            try
            {
                using (var connection = new SqlConnection(_healthCareConnection))
                {
                    using (var command = new SqlCommand("GetOrderMasterByStatus", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        if(status != null)
                        {
                            command.Parameters.Add(new SqlParameter("@Status", status));
                        }

                        await connection.OpenAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var product = new OrderDataBaseOnStatusDto();

                                product.OrderMasterId = Convert.ToInt32(reader["OrderMasterId"]);
                                product.OrderNo = reader["OrderNo"].ToString();
                                product.OrderDate = Convert.ToDateTime(reader["OrderDate"]);
                                product.Address = reader["Address"].ToString();
                                product.UserId = Convert.ToInt32(reader["UserId"]);
                                product.PaymentMethod = reader["PaymentMethod"].ToString();
                                product.NumberOfItem = Convert.ToInt32(reader["NumberOfItem"]);
                                product.TotalPrice = Convert.ToInt32(reader["TotalPrice"]);
                                product.PhoneNumber = reader["PhoneNumber"].ToString();
                                product.DeliveryCharge = Convert.ToDecimal(reader["DeliveryCharge"]);
                                product.Status = reader["Status"].ToString();
                                products.Add(product);
                            }
                        }
                    }
                }

                //if (products.Count == 0)
                //{
                //    return NotFound(new { message = "No Order found for the given Status." });
                //}

                return Ok(products);
            }
            catch (Exception ex)
            {
                // Log the exception here
                return StatusCode(500, "An error occurred while retrieving products: " + ex.Message);
            }
        }
        [HttpGet("GetOrderDetailData")]
        public async Task<IActionResult> GetOrderDetailData(int? OrderMasterId, string? status = null)
        {
            var orderDetails = new List<OrderDetailStatusDto>();

            try
            {
                using (var connection = new SqlConnection(_healthCareConnection))
                {
                    using (var command = new SqlCommand("GetOrderDetailStatus", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        if (status != null)
                        {
                            command.Parameters.Add(new SqlParameter("@Status", status));
                        }
                        command.Parameters.Add(new SqlParameter("@OrderMasterId", OrderMasterId));

                        await connection.OpenAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var orderDetail = new OrderDetailStatusDto();

                                orderDetail.OrderDetailId = reader.IsDBNull(reader.GetOrdinal("OrderDetailId")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("OrderDetailId"));
                                orderDetail.OrderMasterId = reader.IsDBNull(reader.GetOrdinal("OrderMasterId")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("OrderMasterId"));
                                orderDetail.UserId = reader.IsDBNull(reader.GetOrdinal("UserId")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("UserId"));
                                orderDetail.ProductId = reader.IsDBNull(reader.GetOrdinal("ProductId")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("ProductId"));
                                orderDetail.ProductGroupCode = reader.IsDBNull(reader.GetOrdinal("ProductGroupCode")) ? null : reader.GetString(reader.GetOrdinal("ProductGroupCode"));
                                orderDetail.FullName = reader.IsDBNull(reader.GetOrdinal("FullName")) ? null : reader.GetString(reader.GetOrdinal("FullName"));
                                orderDetail.ProductName = reader.IsDBNull(reader.GetOrdinal("ProductName")) ? null : reader.GetString(reader.GetOrdinal("ProductName"));
                                orderDetail.Specification = reader.IsDBNull(reader.GetOrdinal("Specification")) ? null : reader.GetString(reader.GetOrdinal("Specification"));
                                orderDetail.Unit = reader.IsDBNull(reader.GetOrdinal("Unit")) ? null : reader.GetString(reader.GetOrdinal("Unit"));
                                orderDetail.Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? null : reader.GetString(reader.GetOrdinal("Status"));
                                orderDetail.Qty = reader.IsDBNull(reader.GetOrdinal("Qty")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("Qty"));
                                orderDetail.UnitId = reader.IsDBNull(reader.GetOrdinal("UnitId")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("UnitId"));
                                orderDetail.DiscountAmount = reader.IsDBNull(reader.GetOrdinal("DiscountAmount")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("DiscountAmount"));
                                orderDetail.Price = reader.IsDBNull(reader.GetOrdinal("Price")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("Price"));
                                orderDetail.DeliveryCharge = reader.IsDBNull(reader.GetOrdinal("DeliveryCharge")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("DeliveryCharge"));
                                orderDetail.DeliveryDate = reader.IsDBNull(reader.GetOrdinal("DeliveryDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("DeliveryDate"));
                                orderDetail.DiscountPct = reader.IsDBNull(reader.GetOrdinal("DiscountPct")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("DiscountPct"));
                                orderDetail.NetPrice = reader.IsDBNull(reader.GetOrdinal("NetPrice")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("NetPrice"));

                                orderDetails.Add(orderDetail);
                            }
                        }
                    }
                }

                //if (orderDetails.Count == 0)
                //{
                //    return NotFound(new { message = "No Order details found for the given parameters." });
                //}

                return Ok(orderDetails);
            }
            catch (Exception ex)
            {
                // Log the exception here
                return StatusCode(500, "An error occurred while retrieving order details: " + ex.Message);
            }
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


        //made by tushar
        // [HttpPut("AdminOrderUpdateStatus"), Authorize(Roles = "admin")]
        [HttpPut("AdminOrderUpdateStatus")]
        public async Task<IActionResult> UpdateOrderStatusAsync(String orderMasterId, String? detailsCancelledId, string status)
        {
            // Start a transaction
            SqlTransaction transaction = null;
            try
            {
                using SqlConnection con = new SqlConnection(_healthCareConnection);
                await con.OpenAsync();
                transaction = con.BeginTransaction();



                if (!string.IsNullOrEmpty(orderMasterId))
                {
                    string MasterIdString = "''";

                    List<int> MasterIds = orderMasterId.Split(',').Select(int.Parse).ToList();
                    MasterIdString = string.Join(",", MasterIds);
                    string masterStatusChangeQuery = "UPDATE OrderMaster SET Status = @value  WHERE OrderMasterId IN (" + MasterIdString + ") ;";

                    SqlCommand cmd1 = new SqlCommand(masterStatusChangeQuery, con, transaction);
                    //cmd1.Parameters.AddWithValue("@orderMasterId", orderMasterId);
                    cmd1.Parameters.AddWithValue("@value", status);

                    int masteRES = await cmd1.ExecuteNonQueryAsync();
                    if(masteRES > 0)
                    {
                        string detailsStatusChangeQuery = "UPDATE OrderDetails SET Status = @value WHERE OrderMasterId  IN (" + MasterIdString + "); ";
                        SqlCommand cmd2 = new SqlCommand(detailsStatusChangeQuery, con, transaction);
                        cmd2.Parameters.AddWithValue("@value", status);

                        int DetailRES = await cmd2.ExecuteNonQueryAsync();
                        if (masteRES > 0)
                        {

                        }
                        else
                        {
                            // If there is any error, rollback the transaction
                            if (transaction != null)
                            {
                                transaction.Rollback();
                            }
                            return BadRequest(new { message = "Order Details Status is not Changed." });
                        }
                    }
                    else
                    {
                        // If there is any error, rollback the transaction
                        if (transaction != null)
                        {
                            transaction.Rollback();
                        }
                        return BadRequest(new { message = "Order Master Status is not Changed." });
                    }

                }
                else
                {
                    return BadRequest(new { message = "Send A Valid Order Id." });

                }



                string CancelledString = "''";
                string detailsStatus = "Cancelled";
                if (!string.IsNullOrEmpty(detailsCancelledId))
                {
                    List<int> CanncelledIds = detailsCancelledId.Split(',').Select(int.Parse).ToList();
                    CancelledString = string.Join(",", CanncelledIds);
                    string detailStatusChangeToCancelQuery = "UPDATE OrderDetails SET Status = 'Rejected' WHERE OrderDetailId IN (" + CancelledString + "); ";
                    SqlCommand cmd3 = new SqlCommand(detailStatusChangeToCancelQuery, con, transaction);
                    int cancelRES = await cmd3.ExecuteNonQueryAsync();
                    if (cancelRES > 0)
                    {

                    }
                    else
                    {
                        // If there is any error, rollback the transaction
                        if (transaction != null)
                        {
                            transaction.Rollback();
                        }
                        return BadRequest(new { message = "Order Details Status cancel is not Inserted." });
                    }
                }
                else
                {

                }

                // If everything is fine, commit the transaction
                transaction.Commit();
                return Ok(new { message = "Order Status Changed Successfully." });
            }
            catch(Exception ex)
            {
                // If there is any error, rollback the transaction
                if (transaction != null)
                {
                    transaction.Rollback();
                }
                return BadRequest(new { message = ex.Message });
            }
            finally
            {
                // Finally block to ensure the connection is always closed
                if (con.State == ConnectionState.Open)
                {
                    await con.CloseAsync();
                }
            }

        }
        //older status change code
        //public async Task<IActionResult> UpdateOrderStatusAsync(OrderMasterStatusUpdateDto orderMasterStatusUpdate)
        //{
        //    try
        //    {
        //        string orderMasterQuery = @"UPDATE OrderMaster SET Status = @Status, UpdatedDate=@UpdatedDate, UpdatedBy=@UpdatedBy, UpdatedPC=@UpdatedPC WHERE OrderMasterId = @OrderMasterId";
        //        using (var connection = new SqlConnection(_healthCareConnection))
        //        {
        //            await connection.OpenAsync();
        //            using (SqlCommand orderMasterCommand = new SqlCommand(orderMasterQuery, connection))
        //            {
        //                orderMasterCommand.Parameters.AddWithValue("@OrderMasterId", orderMasterStatusUpdate.OrderMasterId);
        //                orderMasterCommand.Parameters.AddWithValue("@Status", orderMasterStatusUpdate.Status);
        //                orderMasterCommand.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);
        //                orderMasterCommand.Parameters.AddWithValue("@UpdatedBy", orderMasterStatusUpdate.UpdatedBy);
        //                orderMasterCommand.Parameters.AddWithValue("@UpdatedPC", orderMasterStatusUpdate.UpdatedPC);
        //                int affectedRows = await orderMasterCommand.ExecuteNonQueryAsync();
        //                if (affectedRows > 0)
        //                {
        //                    await UpdateOrderDetailsStatus(orderMasterStatusUpdate.OrderMasterId, orderMasterStatusUpdate.OrderDetailsStatusUpdatelist);
        //                    return Ok(new { message = "Order status updated successfully." });
        //                }
        //                else
        //                {
        //                    return BadRequest(new { message = "Failed to update order status." });
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { message = $"An error occurred: {ex.Message}" });
        //    }
        //}
        //private async Task UpdateOrderDetailsStatus(int OrderMasterId, List<OrderDetailsStatusUpdateDto> OrderDetailsStatusUpdatelist)
        //{
        //    try
        //    {
        //        foreach (var orderDetailsUpdate in OrderDetailsStatusUpdatelist)
        //        {
        //            string orderDetailsQuery = @"UPDATE OrderDetails SET Status = @Status, UpdatedDate=@UpdatedDate, UpdatedBy=@UpdatedBy, UpdatedPC=@UpdatedPC WHERE OrderMasterId = @OrderMasterId AND OrderDetailId = @OrderDetailId";
        //            using (var connection = new SqlConnection(_healthCareConnection))
        //            {
        //                await connection.OpenAsync();
        //                using (SqlCommand orderDetailsCommand = new SqlCommand(orderDetailsQuery, connection))
        //                {
        //                    orderDetailsCommand.Parameters.AddWithValue("@OrderMasterId", OrderMasterId);
        //                    orderDetailsCommand.Parameters.AddWithValue("@OrderDetailId", orderDetailsUpdate.OrderDetailId);
        //                    orderDetailsCommand.Parameters.AddWithValue("@Status", orderDetailsUpdate.Status);
        //                    orderDetailsCommand.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);
        //                    orderDetailsCommand.Parameters.AddWithValue("@UpdatedBy", orderDetailsUpdate.UpdatedBy);
        //                    orderDetailsCommand.Parameters.AddWithValue("@UpdatedPC", orderDetailsUpdate.UpdatedPC);
        //                    await orderDetailsCommand.ExecuteNonQueryAsync();
        //                }
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Error updating order details status: {ex.Message}");
        //    }
        //}

        [HttpPut("UpdateSellerOrderDetailsStatus")]
        public async Task<IActionResult> SellerOrderDetailsStatusChangedAsync(String orderdetailsIds, string status)
        {

            try
            {
                using (SqlConnection con = new SqlConnection(_healthCareConnection))
                {
                    if (!string.IsNullOrEmpty(orderdetailsIds))
                    {
                        string orderdetailsIdString = "''";

                        List<int> DetailsIds = orderdetailsIds.Split(',').Select(int.Parse).ToList();
                        orderdetailsIdString = string.Join(",", DetailsIds);
                        string masterStatusChangeQuery = "UPDATE OrderDetails SET Status = @value  WHERE OrderDetailId IN (" + orderdetailsIdString + ") ;";

                        SqlCommand cmd1 = new SqlCommand(masterStatusChangeQuery, con);
                        //cmd1.Parameters.AddWithValue("@orderMasterId", orderMasterId);
                        cmd1.Parameters.AddWithValue("@value", status);
                        await con.OpenAsync();
                        int masteRES = await cmd1.ExecuteNonQueryAsync();
                        await con.CloseAsync();
                        if (masteRES > 0)
                        {

                            return Ok(new { message = "Order Status Changed Successfully." });
                        }
                        else
                        {
                            return BadRequest(new { message = "Order Details Status is not Changed." });
                        }

                    }
                    else
                    {
                        return BadRequest(new { message = "Send A Valid OrderDetail Id." });

                    }
                }

            }
            catch (Exception ex)
            {

                return BadRequest(new { message = ex.Message });
            }

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
        [HttpGet("getOrderUserInfo")]
        public IActionResult getUserInfo(string UserId)
        {
            UserModel user = new UserModel();
            SqlConnection con = new SqlConnection(_healthCareConnection);
            SqlCommand cmd = new SqlCommand("SELECT * FROM UserRegistration WHERE UserId = @UserId ", con);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@UserId", UserId);
            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                user.FullName = reader["FullName"].ToString();
                user.PhoneNumber = reader["PhoneNumber"].ToString();
                user.Email = reader["Email"].ToString();
                user.Address = reader["Address"].ToString();
                con.Close();
                return Ok(new { message = "GET single data successful", user });
            }
            else
            {
                con.Close();
                return BadRequest(new { message = "NO data Available" });
            }
        }


        //================================== Added By Tushar ==============================
        [HttpGet("GetSellerOrderBasedOnUserCode")]
        public async Task<IActionResult> GetSellerOrderBasedOnUserCodeAsync(string usercode)
        {
            try
            {
                List<GetSellerOrderBasedOnUserCodeDto> objectlist = new List<GetSellerOrderBasedOnUserCodeDto>();
                using (SqlConnection con = new SqlConnection(_healthCareConnection))
                {
                    string query = "GetSellerSelesBySellerId";
                    SqlCommand sqlCommand = new SqlCommand(query , con);

                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@SellerId", usercode);

                    await con.OpenAsync();
                    SqlDataReader reader = await sqlCommand.ExecuteReaderAsync();
                    if (!reader.HasRows)
                    {
                        return BadRequest(new { message = "No Data Found." });
                    }
                    while (await reader.ReadAsync())
                    {
                        GetSellerOrderBasedOnUserCodeDto details = new GetSellerOrderBasedOnUserCodeDto();
                        {
                            details.OrderNo = reader["OrderNo"].ToString();
                            details.Address = reader["Address"].ToString();
                            details.BUserId = Convert.ToInt32(reader["BUserId"]);
                            details.BuyerName = reader["BuyerName"].ToString();
                            details.ProductGroupID = Convert.ToInt32(reader["ProductGroupID"]);
                            details.ProductId = Convert.ToInt32(reader["ProductId"]);
                            details.ProductName = reader["ProductName"].ToString();
                            details.Specification = reader["Specification"].ToString();
                            details.StockQty = reader.IsDBNull(reader.GetOrdinal("StockQty")) ? (Decimal?)null : (Decimal?)reader.GetDecimal(reader.GetOrdinal("StockQty"));
                            details.SaleQty = reader.IsDBNull(reader.GetOrdinal("SaleQty")) ? (int?)null : (int?)reader.GetInt32(reader.GetOrdinal("SaleQty"));
                            details.UnitId = Convert.ToInt32(reader["UnitId"].ToString());
                            details.Unit = reader["Unit"].ToString();
                            details.NetPrice = reader.IsDBNull(reader.GetOrdinal("NetPrice")) ? (Decimal?)null : (Decimal?)reader.GetDecimal(reader.GetOrdinal("NetPrice"));
                        }

                        objectlist.Add(details);
                    }
                    await con.CloseAsync();
                }

                return Ok(objectlist);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }







        //public class CountsList
        //{
        //    public int PendingCount { get; set; }
        //    public int ProcessingCount { get; set; }
        //    public int ReadyToShipCount { get; set; }
        //    public int ShippedCount { get; set; }
        //    public int DeliveredCount { get; set; }
        //    public int CancelledCount { get; set; }
        //    public int AllCount { get; set; }
        //    public int ToReturnCount { get; set; }
        //    public int ReturnedCount { get; set; }



        //}



        //[HttpGet, Authorize(Roles = "seller")]
        //[Route("getSearchedAllOrderForSeller")]
        //public IActionResult getSearchedAllOrderForSeller(string sellerCode, int PageNumber, int PageSize, String? status = null, String? SearchedOrderNo = null, String? SearchedPaymentMethod = null, String? SearchedStatus = null)
        //{
        //    Console.WriteLine(sellerCode, "sellerCode");
        //    string decryptedSupplierCode = CommonServices.DecryptPassword(sellerCode);
        //    List<SellerOrderMaster> orderLst = new List<SellerOrderMaster>();
        //    List<CountsList> countsList = new List<CountsList>();

        //    SqlConnection con = new SqlConnection(_prominentConnection);
        //    string queryForSeller = "sp_OrderMasterDataForSeller";
        //    con.Open();
        //    SqlCommand cmdForSeller = new SqlCommand(queryForSeller, con);
        //    cmdForSeller.CommandType = CommandType.StoredProcedure;
        //    cmdForSeller.Parameters.AddWithValue("@SellerCode", decryptedSupplierCode);
        //    cmdForSeller.Parameters.AddWithValue("@PageNumber", PageNumber);
        //    cmdForSeller.Parameters.AddWithValue("@PageSize", PageSize);
        //    if (status != null) { cmdForSeller.Parameters.AddWithValue("@Status", status); }
        //    if (SearchedOrderNo != null) { cmdForSeller.Parameters.AddWithValue("@SearchedOrderNo", SearchedOrderNo); }
        //    if (SearchedPaymentMethod != null) { cmdForSeller.Parameters.AddWithValue("@SearchedPaymentMethod", SearchedPaymentMethod); }
        //    if (SearchedStatus != null) { cmdForSeller.Parameters.AddWithValue("@SearchedStatus", SearchedStatus); }
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
        //        order.OrderDate = dt.Rows[i]["OrderDate"].ToString() ;
        //        order.Address = dt.Rows[i]["Address"].ToString();
        //        order.Status = dt.Rows[i]["Status"].ToString();
        //        order.PaymentMethod = dt.Rows[i]["PaymentMethod"].ToString();
        //        order.NumberofItem = Convert.ToInt32(dt.Rows[i]["NumberOfItem"]);
        //        order.TotalPrice = Convert.ToDecimal(dt.Rows[i]["TotalPrice"]);
        //        order.TotalRowCount = Convert.ToInt32(dt.Rows[i]["TotalRowCount"]);
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
        //        counts.ToReturnCount = Convert.ToInt32(dt1.Rows[i]["ToReturnCount"]);
        //        counts.ReturnedCount = Convert.ToInt32(dt1.Rows[i]["ReturnedCount"]);
        //        countsList.Add(counts);
        //    }
        //    return Ok(new { message = "content get successfully", orderLst, countsList });
        //}

        // update order status for seller

        // ============================== Added By Rey ==========================


        // by Marufa





    }
}




