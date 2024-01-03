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
            con = new SqlConnection(_commonServices.HealthCareConnection);

            _healthCareConnection = _commonServices.HealthCareConnection;
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
                transaction.Commit();
                return Ok(new { message = "Order data Inserted Successfully." });
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    transaction.Rollback();
                }
                return BadRequest(new { message = ex.Message });
            }
            finally
            {
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
        }







        [HttpGet("GetOrderMasterData")]
        public async Task<IActionResult> GetOrderMasterData(string? status)
        {
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
                return Ok(products);
            }
            catch (Exception ex)
            {
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
                                // orderDetail.ProductGroupCode = reader.IsDBNull(reader.GetOrdinal("ProductGroupCode")) ? null : reader.GetString(reader.GetOrdinal("ProductGroupCode"));
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

                return Ok(orderDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving order details: " + ex.Message);
            }
        }



        [HttpPost("GetDatailsData"),Authorize(Roles = "admin")]
        public IActionResult GetDatailsData([FromForm] int OrderMasterId)
        {
            SqlConnection con = new SqlConnection(_prominentConnection);

            try
            {
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
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred while fetching the order details data." });
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }



        //made by tushar
        // [HttpPut("AdminOrderUpdateStatus"), Authorize(Roles = "admin")]
        [HttpPut("AdminOrderUpdateStatus")]
        public async Task<IActionResult> UpdateOrderStatusAsync(String orderMasterId, String? detailsCancelledId, string status)
        {
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
                            if (transaction != null)
                            {
                                transaction.Rollback();
                            }
                            return BadRequest(new { message = "Order Details Status is not Changed." });
                        }
                    }
                    else
                    {
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
                transaction.Commit();
                return Ok(new { message = "Order Status Changed Successfully." });
            }
            catch(Exception ex)
            {
                if (transaction != null)
                {
                    transaction.Rollback();
                }
                return BadRequest(new { message = ex.Message });
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    await con.CloseAsync();
                }
            }

        }


       public class updateOrderClass{
            public string? orderdetailsIds { get; set; }
            public string? status { get; set; }
            public SellerSalesMasterDto? sellerSalesMasterDto { get; set; }
        }

        [HttpPut("UpdateSellerOrderDetailsStatus")]
        public async Task<IActionResult> SellerOrderDetailsStatusChangedAsync(updateOrderClass updateOrder)
        {
            SqlTransaction transaction = null;

            try
            {
                using (SqlConnection con = new SqlConnection(_healthCareConnection))
                {
                    if (!string.IsNullOrEmpty(updateOrder.orderdetailsIds))
                    {
                        string orderdetailsIdString = "''";

                        List<int> DetailsIds = updateOrder.orderdetailsIds.Split(',').Select(int.Parse).ToList();
                        orderdetailsIdString = string.Join(",", DetailsIds);
                        string masterStatusChangeQuery = "UPDATE OrderDetails SET Status = @value  WHERE OrderDetailId IN (" + orderdetailsIdString + ") ;";



                        await con.OpenAsync();
                        transaction = con.BeginTransaction();
                        SqlCommand cmd1 = new SqlCommand(masterStatusChangeQuery, con, transaction);
                        cmd1.Parameters.AddWithValue("@value", updateOrder.status);

                        int masteRES = await cmd1.ExecuteNonQueryAsync();
                        if (masteRES > 0)
                        {
                            if (updateOrder.status == "Processing")
                            {
                                var detailsResult = await InsertSellerSalesDataAsync(updateOrder.sellerSalesMasterDto, con, transaction);
                                if (detailsResult is BadRequestObjectResult)
                                {
                                    throw new Exception((detailsResult as BadRequestObjectResult).Value.ToString());
                                }
                            }
                            transaction.Commit();
                            return Ok(new { message = "Order Status Changed Successfully." });
                        }
                        else
                        {
                            return BadRequest(new { message = "Order Details not found." });
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
                if (transaction != null)
                {
                    transaction.Rollback();
                }
                return BadRequest(new { message = ex.Message });
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    await con.CloseAsync();
                }
            }

        }

        private async Task<IActionResult> InsertSellerSalesDataAsync(SellerSalesMasterDto sellerSalesMasterDto, SqlConnection con,  SqlTransaction transaction)
        {
            try
            {
                string systemCode = string.Empty;

                SqlCommand cmdSP = new SqlCommand("spMakeSystemCode", con, transaction);
                {
                    cmdSP.CommandType = CommandType.StoredProcedure;
                    cmdSP.Parameters.AddWithValue("@TableName", "SellerSalesMaster");
                    cmdSP.Parameters.AddWithValue("@Date", DateTime.Now.ToString("yyyy-MM-dd"));
                    cmdSP.Parameters.AddWithValue("@AddNumber", 1);
                    var tempSystem = await cmdSP.ExecuteScalarAsync();
                    systemCode = tempSystem?.ToString() ?? string.Empty;
                }
                int SSMId = int.Parse(systemCode.Split('%')[0]);
                string SSMCode = systemCode.Split('%')[1];

                SqlCommand cmdMaster = new SqlCommand("InsertSellerSalesMaster", con, transaction);
                cmdMaster.CommandType = CommandType.StoredProcedure;



                cmdMaster.Parameters.AddWithValue("@SSMId", SSMId);
                cmdMaster.Parameters.AddWithValue("@SSMCode", SSMCode);
                cmdMaster.Parameters.AddWithValue("@SSMDate", DateTime.Now);
                cmdMaster.Parameters.AddWithValue("@UserId", sellerSalesMasterDto.UserId);
                cmdMaster.Parameters.AddWithValue("@TotalPrice", sellerSalesMasterDto.TotalPrice);
                cmdMaster.Parameters.AddWithValue("@Challan", sellerSalesMasterDto.Challan ?? (object)DBNull.Value);
                cmdMaster.Parameters.AddWithValue("@Remarks", sellerSalesMasterDto.Remarks ?? (object)DBNull.Value);
                cmdMaster.Parameters.AddWithValue("@BUserId", sellerSalesMasterDto.BUserId);
                cmdMaster.Parameters.AddWithValue("@AddedBy", sellerSalesMasterDto.AddedBy);
                cmdMaster.Parameters.AddWithValue("@DateAdded", DateTime.Now);
                cmdMaster.Parameters.AddWithValue("@AddedPC", sellerSalesMasterDto.AddedPC);

                int a = await cmdMaster.ExecuteNonQueryAsync();
                if (a > 0)
                {
                    for (int i = 0; i < sellerSalesMasterDto.SellerSalesDetailsList.Count; i++)
                    {
                        string detailsQuery = "InsertSellerSalesDetail";
                        SqlCommand cmdDetails = new SqlCommand(detailsQuery, con, transaction);
                        cmdDetails.CommandType = CommandType.StoredProcedure;

                        cmdDetails.Parameters.Clear();

                        cmdDetails.Parameters.AddWithValue("@SSMId", SSMId);
                        cmdDetails.Parameters.AddWithValue("@OrderNo", sellerSalesMasterDto.SellerSalesDetailsList[i].OrderNo);
                        cmdDetails.Parameters.AddWithValue("@ProductId", sellerSalesMasterDto.SellerSalesDetailsList[i].ProductId);
                        cmdDetails.Parameters.AddWithValue("@Specification", sellerSalesMasterDto.SellerSalesDetailsList[i].Specification);
                        cmdDetails.Parameters.AddWithValue("@StockQty", sellerSalesMasterDto.SellerSalesDetailsList[i].StockQty);
                        cmdDetails.Parameters.AddWithValue("@SaleQty", sellerSalesMasterDto.SellerSalesDetailsList[i].SaleQty);
                        cmdDetails.Parameters.AddWithValue("@UnitId", sellerSalesMasterDto.SellerSalesDetailsList[i].UnitId);
                        cmdDetails.Parameters.AddWithValue("@NetPrice", sellerSalesMasterDto.SellerSalesDetailsList[i].NetPrice);
                        cmdDetails.Parameters.AddWithValue("@Address", sellerSalesMasterDto.SellerSalesDetailsList[i].Address);
                        cmdDetails.Parameters.AddWithValue("@ProductGroupID", sellerSalesMasterDto.SellerSalesDetailsList[i].ProductGroupID);
                        cmdDetails.Parameters.AddWithValue("@Remarks", sellerSalesMasterDto.SellerSalesDetailsList[i].Remarks ?? (object)DBNull.Value);

                        cmdDetails.Parameters.AddWithValue("@AddedBy", sellerSalesMasterDto.SellerSalesDetailsList[i].AddedBy);
                        cmdDetails.Parameters.AddWithValue("@AddedDate", DateTime.Now);
                        cmdDetails.Parameters.AddWithValue("@AddedPC", sellerSalesMasterDto.SellerSalesDetailsList[i].AddedPC);

                        int detailsRes = await cmdDetails.ExecuteNonQueryAsync();
                        if (detailsRes<=0)
                        {
                            return BadRequest(new { message = "SellerSales details data isn't Inserted." });
                        }
                    }
                }
                else
                {
                    return BadRequest(new { message = "SellerSales Master data isn't Inserted Successfully." });
                }
                return Ok(new { message = "SellerSale data Inserted Successfully." });

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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
                    ordersData.Add(modelObj);
                }
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
        }



        //================================== Added By Rey ==============================
        [HttpGet("getOrderUserInfo")]
        public IActionResult getUserInfo(string UserId)
        {
            UserModel user = new UserModel();
            SqlConnection con = new SqlConnection(_healthCareConnection);

            try
            {
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
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred while fetching user information." });
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }


        //================================== Added By Tushar ==============================
        [HttpGet("GetSellerOrderBasedOnUserID")]
        public async Task<IActionResult> GetSellerOrderBasedOnUserCodeAsync(string userid, string? status)
        {
            try
            {
                List<GetSellerOrderBasedOnUserCodeDto> objectlist = new List<GetSellerOrderBasedOnUserCodeDto>();
                using (SqlConnection con = new SqlConnection(_healthCareConnection))
                {
                    string query = "GetSellerSelesBySellerId";
                    SqlCommand sqlCommand = new SqlCommand(query , con);

                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@SellerId", userid);
                    if(status != null)
                    {
                        sqlCommand.Parameters.AddWithValue("@Status", status);
                    }


                    await con.OpenAsync();
                    SqlDataReader reader = await sqlCommand.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        GetSellerOrderBasedOnUserCodeDto details = new GetSellerOrderBasedOnUserCodeDto();
                        {
                            details.OrderDetailId = Convert.ToInt32(reader["OrderDetailId"].ToString());
                            details.OrderMasterId = Convert.ToInt32(reader["OrderMasterId"].ToString());

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
                            details.Status = reader["Status"].ToString();
                            details.ReturnTypeName = reader["ReturnTypeName"] is DBNull || reader["ReturnTypeName"].ToString() == null? (string?)null: reader["ReturnTypeName"].ToString();



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


        [HttpGet("GetBuyerOrderBasedOnUserID")]
        public async Task<IActionResult> GetBuyerOrderBasedOnUserIDAsync(string userid, string? status)
        {
            try
            {
                List<GetBuyerOrderBasedOnUserIDDto> objectlist = new List<GetBuyerOrderBasedOnUserIDDto>();
                using (SqlConnection con = new SqlConnection(_healthCareConnection))
                {
                    string query = "GetBuyerOrderByUserId";
                    SqlCommand sqlCommand = new SqlCommand(query, con);

                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@UserId", userid);
                    if (status != null)
                    {
                        sqlCommand.Parameters.AddWithValue("@Status", status);
                    }


                    await con.OpenAsync();
                    SqlDataReader reader = await sqlCommand.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        GetBuyerOrderBasedOnUserIDDto details = new GetBuyerOrderBasedOnUserIDDto();
                        {
                            details.OrderDetailId = Convert.ToInt32(reader["OrderDetailId"].ToString());
                            details.OrderMasterId = Convert.ToInt32(reader["OrderMasterId"].ToString());
                            details.OrderNo = reader["OrderNo"].ToString();
                            details.OrderDate = reader.IsDBNull(reader.GetOrdinal("OrderDate")) ? (DateTime?)null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("OrderDate"));
                            details.Address = reader["Address"].ToString();
                            details.BuyerName = reader["BuyerName"].ToString();
                            details.PaymentMethod = reader["PaymentMethod"].ToString();
                            details.NumberOfItem = Convert.ToInt32(reader["NumberOfItem"]);
                            details.TotalPrice = Convert.ToInt32(reader["TotalPrice"]);
                            details.PhoneNumber = reader["PhoneNumber"].ToString();
                            details.DeliveryCharge = reader.IsDBNull(reader.GetOrdinal("DeliveryCharge")) ? (Decimal?)null : (Decimal?)reader.GetDecimal(reader.GetOrdinal("DeliveryCharge"));
                            details.ProductName = reader["ProductName"].ToString();
                            details.Specification = reader["Specification"].ToString();
                            details.Qty = Convert.ToInt32(reader["Qty"].ToString());
                            details.UnitId = Convert.ToInt32(reader["UnitId"].ToString());
                            details.Unit = reader["Unit"].ToString();
                            details.DiscountAmount = reader.IsDBNull(reader.GetOrdinal("DiscountAmount")) ? (Decimal?)null : (Decimal?)reader.GetDecimal(reader.GetOrdinal("DiscountAmount"));
                            details.Price = reader.IsDBNull(reader.GetOrdinal("Price")) ? (Decimal?)null : (Decimal?)reader.GetDecimal(reader.GetOrdinal("Price"));
                            details.DetailDeliveryCharge = reader.IsDBNull(reader.GetOrdinal("DetailDeliveryCharge")) ? (Decimal?)null : (Decimal?)reader.GetDecimal(reader.GetOrdinal("DetailDeliveryCharge"));
                            details.DetailDeliveryDate = reader.IsDBNull(reader.GetOrdinal("DetailDeliveryDate")) ? (DateTime?)null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("DetailDeliveryDate"));
                            details.DiscountPct = reader.IsDBNull(reader.GetOrdinal("DiscountPct")) ? (Decimal?)null : (Decimal?)reader.GetDecimal(reader.GetOrdinal("DiscountPct"));
                            details.NetPrice = reader.IsDBNull(reader.GetOrdinal("NetPrice")) ? (Decimal?)null : (Decimal?)reader.GetDecimal(reader.GetOrdinal("NetPrice"));
                            details.OrderStatus = reader["OrderStatus"].ToString();
                            details.SellerStatus = reader["SellerStatus"].ToString();

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


        [HttpGet("getAllOrderForBuyer")]
        public async Task<IActionResult> getAllOrderForBuyerAsync(string userid, string? status)
        {
            List<OrderMasterDataForBuyerDto> MasterList = new List<OrderMasterDataForBuyerDto>();

            try
            {
                string query = string.Empty;

                if (status != null)
                {
                    query = @"select OM.OrderMasterId, OM.OrderNo, OM.OrderDate, OD.OrderDetailId, OD.ProductId, PL.ProductName, 
                                  PL.ImagePath, OD.Qty, OD.Status  from OrderMaster OM
                                  join OrderDetails OD on OM.OrderMasterId = OD.OrderMasterId
                                  join ProductList PL on PL.ProductId = OD.ProductId
                                  where OM.UserId = @UserId and OD.Status = @Status ORDER BY OM.OrderMasterId DESC;";
                }
                else
                {
                    query = @"select OM.OrderMasterId, OM.OrderNo, OM.OrderDate, OD.OrderDetailId, OD.ProductId, PL.ProductName, 
                                  PL.ImagePath, OD.Qty, OD.Status  from OrderMaster OM
                                  join OrderDetails OD on OM.OrderMasterId = OD.OrderMasterId
                                  join ProductList PL on PL.ProductId = OD.ProductId
                                  where OM.UserId = @UserId ORDER BY OM.OrderMasterId DESC;";
                }

                con.Open();
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.Text;
                    if (status != null)
                    {
                        cmd.Parameters.AddWithValue("@UserId", userid);
                        cmd.Parameters.AddWithValue("@Status", status);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@UserId", userid);
                    }

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        OrderMasterDataForBuyerDto Master = null;

                        while (reader.Read())
                        {
                            int OrderMasterId = reader.IsDBNull("OrderMasterId") ? -1 : Convert.ToInt32(reader["OrderMasterId"]);

                            if (Master == null || Master.OrderMasterId != OrderMasterId)
                            {
                                Master = new OrderMasterDataForBuyerDto();
                                Master.OrderMasterId = OrderMasterId;
                                Master.OrderNo = reader.IsDBNull("OrderNo") ? (string?)null : reader["OrderNo"].ToString();
                                Master.OrderDate = reader.IsDBNull(reader.GetOrdinal("OrderDate")) ? (DateTime?)null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("OrderDate"));

                                MasterList.Add(Master);

                                OrderDetailsDataForBuyerDto Detail = new OrderDetailsDataForBuyerDto()
                                {

                                    OrderDetailId = reader.IsDBNull("OrderDetailId") ? (int?)null : Convert.ToInt32(reader["OrderDetailId"]),
                                    ProductId = reader.IsDBNull("ProductId") ? (int?)null : Convert.ToInt32(reader["ProductId"]),
                                    ProductName = reader.IsDBNull("ProductName") ? null : reader["ProductName"].ToString(),
                                    ImagePath = reader.IsDBNull("ImagePath") ? null : reader["ImagePath"].ToString(),
                                    Qty = reader.IsDBNull("Qty") ? (int?)null : Convert.ToInt32(reader["Qty"]),
                                    Status = reader.IsDBNull("Status") ? null : reader["Status"].ToString()
                                };
                                Master.OrderDetailsListForBuyer.Add(Detail);

                            }
                            else
                            {
                                OrderDetailsDataForBuyerDto Detail = new OrderDetailsDataForBuyerDto()
                                {

                                    OrderDetailId = reader.IsDBNull("OrderDetailId") ? (int?)null : Convert.ToInt32(reader["OrderDetailId"]),
                                    ProductId = reader.IsDBNull("ProductId") ? (int?)null : Convert.ToInt32(reader["ProductId"]),
                                    ProductName = reader.IsDBNull("ProductName") ? null : reader["ProductName"].ToString(),
                                    ImagePath = reader.IsDBNull("ImagePath") ? null : reader["ImagePath"].ToString(),
                                    Qty = reader.IsDBNull("Qty") ? (int?)null : Convert.ToInt32(reader["Qty"]),
                                    Status = reader.IsDBNull("Status") ? null : reader["Status"].ToString()
                                };
                                Master.OrderDetailsListForBuyer.Add(Detail);
                            }
                        }
                    }
                }
                return Ok(MasterList);
            }
            catch (Exception ex)
            {
                // Handle the exception
                return null;
            }
            finally
            {
                con.Close();
            }

        }

        [HttpGet("getOrderDetailsForBuyerBasedOnOrderNo")]
        public async Task<IActionResult> getOrderDetailsForBuyerBasedOnOrderNoAsync(string OrderNo)
        {
            //OrderDetailsMasterForBuyerDto Master = new OrderDetailsMasterForBuyerDto();
            OrderDetailsMasterForBuyerDto Master = null;
            try
            {
                string query = @"SELECT
                      OM.OrderMasterId,
                      OM.OrderNo,
                      OM.OrderDate,
                      OD.UserId AS SellerID,
                      UR.FullName AS SellerName,
                      OD.DeliveryDate,
                      PL.ProductName,
                      PL.ImagePath,
                      SPP.TotalPrice AS Price,
                      OD.Qty As TotalQty,
                      OD.Status,
                      OD.DeliveryCharge,
                      OM.PaymentMethod,
                      UR2.FullName BuyerName,
                      OM.Address As ShippingAddress,
                      OM.PhoneNumber As ShippingPhoneNumber,
                      UR2.Address As BillingAddress,
                      UR2.PhoneNumber As BillingPhoneNumber
                    FROM
                      OrderMaster OM
                      LEFT JOIN OrderDetails OD ON OD.OrderMasterId = OM.OrderMasterId
                      LEFT JOIN UserRegistration UR ON UR.UserId = OD.UserId
                      LEFT JOIN ProductList PL ON PL.ProductId = OD.ProductId
                      LEFT JOIN SellerProductPriceAndOffer SPP ON SPP.ProductId = OD.ProductId AND SPP.UserId = OD.UserId
                      LEFT JOIN UserRegistration UR2 ON UR2.UserId= OM.UserId
                    WHERE
                      OM.OrderNo = @OrderNo
                    GROUP BY
                      OM.OrderMasterId,
                      OM.OrderNo,
                      OM.OrderDate,
                      OD.UserId,
                      UR.FullName,
                      PL.ProductName,
                      PL.ImagePath,
                      OD.DeliveryDate,
                      SPP.TotalPrice,
                      OD.Qty,
                      OD.Status,
                      OD.DeliveryCharge,
                      OM.Address,
                      OM.PhoneNumber,
                      OM.PaymentMethod,
                      UR2.FullName,
                      UR2.Address,
                      UR2.PhoneNumber"
                ;


                con.Open();
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@OrderNo", OrderNo);


                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {

                        OrderDetails2ndMasterForBuyerDto SecondMaster = null;
                        int? sellermaster = null;
                        while (reader.Read())
                        {

                            decimal? Packagesubtotal = 0;
                            int OrderMasterId = reader.IsDBNull("OrderMasterId") ? -1 : Convert.ToInt32(reader["OrderMasterId"]);
                            //OrderDetails2ndMasterForBuyerDto SecondMaster = new OrderDetails2ndMasterForBuyerDto();

                            if (Master == null || Master.OrderMasterId != OrderMasterId)
                            {
                                SecondMaster = new OrderDetails2ndMasterForBuyerDto();
                                Master = new OrderDetailsMasterForBuyerDto();
                                Master.OrderMasterId = OrderMasterId;
                                Master.OrderNo = reader.IsDBNull("OrderNo") ? (string?)null : reader["OrderNo"].ToString();
                                Master.OrderDate = reader.IsDBNull(reader.GetOrdinal("OrderDate")) ? (DateTime?)null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("OrderDate"));
                                Master.PaymentMethod = reader.IsDBNull("PaymentMethod") ? (string?)null : reader["PaymentMethod"].ToString();
                                Master.BuyerName = reader.IsDBNull("BuyerName") ? (string?)null : reader["BuyerName"].ToString();
                                Master.ShippingAddress = reader.IsDBNull("ShippingAddress") ? (string?)null : reader["ShippingAddress"].ToString();
                                Master.ShippingPhoneNumber = reader.IsDBNull("ShippingPhoneNumber") ? (string?)null : reader["ShippingPhoneNumber"].ToString();
                                Master.BillingAddress = reader.IsDBNull("BillingAddress") ? (string?)null : reader["BillingAddress"].ToString();
                                Master.BillingPhoneNumber = reader.IsDBNull("BillingPhoneNumber") ? (string?)null : reader["BillingPhoneNumber"].ToString();



                                sellermaster = reader.IsDBNull("SellerId") ? (int?)null : Convert.ToInt32(reader["SellerId"]);
                                SecondMaster.SellerId = reader.IsDBNull("SellerId") ? (int?)null : Convert.ToInt32(reader["SellerId"]);
                                SecondMaster.DeliveryDate = reader.IsDBNull(reader.GetOrdinal("DeliveryDate")) ? (DateTime?)null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("DeliveryDate"));
                                SecondMaster.Status = reader.IsDBNull("Status") ? null : reader["Status"].ToString();
                                //SecondMaster.PackageSubtotal += reader.IsDBNull(reader.GetOrdinal("Price")) ? (decimal?)null : (decimal?)reader.GetDecimal(reader.GetOrdinal("Price"));
                                SecondMaster.PackageDeliveryCharge = reader.IsDBNull("DeliveryCharge") ? (int?)null : Convert.ToInt32(reader["DeliveryCharge"]);
                                Master.TotalDeliveryCharge += SecondMaster.PackageDeliveryCharge;
                                //SecondMaster.Status = reader.IsDBNull("Status") ? null : reader["Status"].ToString();




                                OrderDetails2ndMDetailsForBuyerDto SecondMasterDetails = new OrderDetails2ndMDetailsForBuyerDto();

                                SecondMasterDetails.Imagepath = reader.IsDBNull("Imagepath") ? null : reader["Imagepath"].ToString();
                                SecondMasterDetails.ProductName = reader.IsDBNull("ProductName") ? null : reader["ProductName"].ToString();
                                SecondMasterDetails.Price = reader.IsDBNull("Price") ? (int?)null : Convert.ToInt32(reader["Price"]);
                                SecondMasterDetails.TotalQty = reader.IsDBNull("TotalQty") ? (int?)null : Convert.ToInt32(reader["TotalQty"]);
                                SecondMaster.PackageSubtotal += SecondMasterDetails.Price * SecondMasterDetails.TotalQty;
                                Master.SubTotal += SecondMaster.PackageSubtotal;
                                Master.TotalAmount += Master.SubTotal + Master.TotalDeliveryCharge;

                                SecondMaster.OrderDetails2ndMDetailsListForBuyer.Add(SecondMasterDetails);

                                Master.OrderDetails2ndMasterListForBuyer.Add(SecondMaster);

                            }
                            else
                            {
                                OrderDetails2ndMasterForBuyerDto SenMaster = new OrderDetails2ndMasterForBuyerDto();

                                int? SellerId = reader.IsDBNull("SellerId") ? (int?)null : Convert.ToInt32(reader["SellerId"]);
                                if (SellerId == sellermaster)
                                {
                                    OrderDetails2ndMDetailsForBuyerDto SecondMasterDetails = new OrderDetails2ndMDetailsForBuyerDto();

                                    SecondMasterDetails.Imagepath = reader.IsDBNull("Imagepath") ? null : reader["Imagepath"].ToString();
                                    SecondMasterDetails.ProductName = reader.IsDBNull("ProductName") ? null : reader["ProductName"].ToString();
                                    SecondMasterDetails.Price = reader.IsDBNull("Price") ? (int?)null : Convert.ToInt32(reader["Price"]);
                                    SecondMasterDetails.TotalQty = reader.IsDBNull("TotalQty") ? (int?)null : Convert.ToInt32(reader["TotalQty"]);
                                    SecondMaster.PackageSubtotal += SecondMasterDetails.Price * SecondMasterDetails.TotalQty;
                                    //Master.SubTotal += SecondMaster.PackageSubtotal;
                                    //Master.TotalAmount += Master.SubTotal + Master.TotalDeliveryCharge;

                                    SecondMaster.OrderDetails2ndMDetailsListForBuyer.Add(SecondMasterDetails);

                                }
                                else
                                {
                                    SenMaster.SellerId = reader.IsDBNull("SellerId") ? (int?)null : Convert.ToInt32(reader["SellerId"]);
                                    SenMaster.DeliveryDate = reader.IsDBNull(reader.GetOrdinal("DeliveryDate")) ? (DateTime?)null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("DeliveryDate"));
                                    SenMaster.Status = reader.IsDBNull("Status") ? null : reader["Status"].ToString();
                                    //SenMaster.PackageSubtotal += reader.IsDBNull(reader.GetOrdinal("Price")) ? (decimal?)null : (decimal?)reader.GetDecimal(reader.GetOrdinal("Price"));
                                    SenMaster.PackageDeliveryCharge = reader.IsDBNull("DeliveryCharge") ? (int?)null : Convert.ToInt32(reader["DeliveryCharge"]);
                                    //SenMaster.Status = reader.IsDBNull("Status") ? null : reader["Status"].ToString();




                                    OrderDetails2ndMDetailsForBuyerDto SecondMasterDetails = new OrderDetails2ndMDetailsForBuyerDto();

                                    SecondMasterDetails.Imagepath = reader.IsDBNull("Imagepath") ? null : reader["Imagepath"].ToString();
                                    SecondMasterDetails.ProductName = reader.IsDBNull("ProductName") ? null : reader["ProductName"].ToString();
                                    SecondMasterDetails.Price = reader.IsDBNull("Price") ? (int?)null : Convert.ToInt32(reader["Price"]);
                                    SecondMasterDetails.TotalQty = reader.IsDBNull("TotalQty") ? (int?)null : Convert.ToInt32(reader["TotalQty"]);
                                    SenMaster.PackageSubtotal += SecondMasterDetails.Price * SecondMasterDetails.TotalQty;
                                    Master.TotalDeliveryCharge += SecondMaster.PackageDeliveryCharge;
                                    Master.TotalAmount += Master.SubTotal + Master.TotalDeliveryCharge;

                                    SenMaster.OrderDetails2ndMDetailsListForBuyer.Add(SecondMasterDetails);
                                    Master.OrderDetails2ndMasterListForBuyer.Add(SenMaster);
                                }



                            }
                        }
                    }
                }
                return Ok(Master);
            }
            catch (Exception ex)
            {
                // Handle the exception
                return null;
            }
            finally
            {
                con.Close();
            }

        }

    }
}




