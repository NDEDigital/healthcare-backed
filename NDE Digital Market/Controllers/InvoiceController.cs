using Microsoft.AspNetCore.Mvc;
using NDE_Digital_Market.Model.OrderModel;
using NDE_Digital_Market.SharedServices;
using System.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using NDE_Digital_Market.DTOs;

namespace NDE_Digital_Market.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : Controller
    {

        private readonly string _connectionSteel;
        private readonly string _connectionDigitalMarket;
        private readonly string connectionHealthCare;
        public InvoiceController(IConfiguration config)
        {
            CommonServices commonServices = new CommonServices(config);
            _connectionSteel = config.GetConnectionString("DefaultConnection");
            _connectionDigitalMarket = config.GetConnectionString("DigitalMarketConnection");
            connectionHealthCare = commonServices.HealthCareConnection;

        }

        //========================================== Added By Maru =================================
        // Get Invoice data For Admin
        //[HttpGet, Authorize(Roles = "admin")]
        [HttpGet]
        [Route("GetInvoiceDataForAdmin")]
        public async Task<IActionResult> GetInvoiceDataForAdminAsync(int OrderMasterId)
        {
            try
            {
                List<GetOrderInvoiceByMasterIdDto> objectlist = new List<GetOrderInvoiceByMasterIdDto>();
                using (SqlConnection con = new SqlConnection(connectionHealthCare))
                {
                    string query = "GetOrderInvoiceByMasterId";
                    SqlCommand sqlCommand = new SqlCommand(query , con);

                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@OrderMasterId", OrderMasterId);

                    await con.OpenAsync();
                    SqlDataReader reader = await sqlCommand.ExecuteReaderAsync();
                    if (!reader.HasRows)
                    {
                        return BadRequest(new { message = "No Order Data Found." });
                    }
                    while (await reader.ReadAsync())
                    {
                        GetOrderInvoiceByMasterIdDto details = new GetOrderInvoiceByMasterIdDto();
                        {
                            details.OrderMasterId = Convert.ToInt32(reader["OrderMasterId"].ToString());
                            details.OrderNo = reader["OrderNo"].ToString();
                            details.OrderDate = Convert.ToDateTime(reader["OrderDate"].ToString());
                            details.Address = reader["Address"].ToString();
                            details.BuyerName = reader["BuyerName"].ToString();
                            details.PaymentMethod = reader["PaymentMethod"].ToString();
                            details.NumberOfItem = Convert.ToInt32(reader["NumberOfItem"].ToString());
                            details.TotalPrice = Convert.ToDecimal(reader["TotalPrice"].ToString());
                            details.PhoneNumber = reader["PhoneNumber"].ToString();
                            details.DeliveryCharge = Convert.ToDecimal(reader["DeliveryCharge"].ToString());
                            details.OrderDetailId = Convert.ToInt32(reader["OrderDetailId"].ToString());
                            details.SellerName = reader["SellerName"].ToString();
                            details.ProductName = reader["ProductName"].ToString();
                            details.Specification = reader["Specification"].ToString();
                            details.Qty = Convert.ToInt32(reader["Qty"].ToString());
                            details.UnitId = Convert.ToInt32(reader["UnitId"].ToString());
                            details.Unit = reader["Unit"].ToString();
                            details.DiscountAmount = reader.IsDBNull(reader.GetOrdinal("DiscountAmount")) ? (Decimal?)null : (Decimal?)reader.GetDecimal(reader.GetOrdinal("DiscountAmount"));
                            details.Price = Convert.ToDecimal(reader["Price"].ToString());
                            details.DetailDeliveryCharge = Convert.ToDecimal(reader["DetailDeliveryCharge"].ToString());
                            //details.DetailDeliveryDate = Convert.ToDateTime(reader["DetailDeliveryDate"].ToString());
                            details.DetailDeliveryDate = reader.IsDBNull(reader.GetOrdinal("DetailDeliveryDate")) ? (DateTime?)null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("DetailDeliveryDate"));
                            details.DiscountPct = reader.IsDBNull(reader.GetOrdinal("DiscountPct")) ? (Decimal?)null : (Decimal?)reader.GetDecimal(reader.GetOrdinal("DiscountPct"));
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

        //public IActionResult GetInvoiceDataForAdmin(int OrderID)
        //{

        //    AdminOrderInVoiceModel invoice = new AdminOrderInVoiceModel();
        //    SqlConnection con = new SqlConnection(_connectionDigitalMarket);
        //    string queryForAdmin = "sp_InvoiceForAdmin";
        //    con.Open();
        //    SqlCommand cmdForSeller = new SqlCommand(queryForAdmin, con);
        //    cmdForSeller.CommandType = CommandType.StoredProcedure;

        //    cmdForSeller.Parameters.AddWithValue("@OrderID", OrderID);
        //    SqlDataAdapter adapter = new SqlDataAdapter(cmdForSeller);
        //    DataSet ds = new DataSet();
        //    adapter.Fill(ds);
        //    DataTable dt = ds.Tables[0];
        //    DataTable dt1 = ds.Tables[1];
        //    DataTable dt2 = ds.Tables[2];
        //    con.Close();
        //    for (int i = 0; i < dt.Rows.Count; i++)
        //    {
        //        AdminSellerDetails sellerdata = new AdminSellerDetails
        //        {
        //            InvoiceNumber = dt.Rows[i]["InvoiceNumber"].ToString(),
        //            GenerateDate = dt.Rows[i]["generateDate"].ToString(),
        //            DeliveryDate = dt.Rows[i]["generateDate"].ToString(),

        //            SellerName = dt.Rows[i]["SellerName"].ToString(),
        //            SellerCompanyName = dt.Rows[i]["SellerCompanyName"].ToString(),
        //            SellerAddress = dt.Rows[i]["SellerAddress"].ToString(),
        //            SellerPhone = dt.Rows[i]["SellerPhone"].ToString(),
        //            SellerCode = dt.Rows[i]["SellerCode"].ToString(),


        //        };
        //        invoice.SellerDetailsList.Add(sellerdata);
        //    }
        //    for (int i = 0; i < dt1.Rows.Count; i++)
        //    {
        //        invoice.OrderNo = dt1.Rows[i]["OrderNo"].ToString();
        //        invoice.OrderDate = dt1.Rows[i]["OrderDate"].ToString();
        //        invoice.BuyerName = dt1.Rows[i]["BuyerName"].ToString();
        //        invoice.TotalPrice = Convert.ToSingle(dt1.Rows[i]["TotalPrice"]).ToString();
        //        invoice.BuyerAddress = dt1.Rows[i]["BuyerAddress"].ToString();
        //        invoice.BuyerPhone = dt1.Rows[i]["BuyerPhone"].ToString();
        //    }
        //    for (int i = 0; i < dt2.Rows.Count; i++)
        //    {
        //        AdminProductDetails product = new AdminProductDetails
        //        {
        //            ProductName = dt2.Rows[i]["ProductName"].ToString(),
        //            Specification = dt2.Rows[i]["Specification"].ToString(),
        //            SellerCode = dt2.Rows[i]["SellerCode"].ToString(),
        //            Quantity = Convert.ToInt32(dt2.Rows[i]["Quantity"]),
        //            Price = Convert.ToSingle(dt2.Rows[i]["Price"]),
        //            Discount = Convert.ToSingle(dt2.Rows[i]["Discount"]),
        //            DeliveryCharge = Convert.ToSingle(dt2.Rows[i]["DeliveryCharge"]),
        //            SubTotalPrice = Convert.ToSingle(dt2.Rows[i]["SubTotalPrice"]),
        //            //       OrderDetailId = Convert.ToInt32(dt.Rows[i]["OrderDetailId"]),
        //        };
        //        // Add the ProductDetails object to the productDetailsList
        //        invoice.ProductDetailsList.Add(product);
        //    }
        //    return Ok(new { message = "Sellers Order Invoice got successfully", invoice });
        //}

        //========================================== Added By Rey =================================
        // Get Invoice data For Seller


        [HttpGet, Authorize(Roles = "seller")]
        [Route("GetInvoiceDataForSeller")]
        public IActionResult GetInvoiceDataForSeller(string sellerCode, int OrderID)
        {
            string decryptedSupplierCode = CommonServices.DecryptPassword(sellerCode);
            SellerInvoiceModel invoice = new SellerInvoiceModel();
            SqlConnection con = new SqlConnection(_connectionDigitalMarket);
            string queryForSeller = "sp_InvoiceDataForSeller";
            con.Open();
            SqlCommand cmdForSeller = new SqlCommand(queryForSeller, con);
            cmdForSeller.CommandType = CommandType.StoredProcedure;
            cmdForSeller.Parameters.AddWithValue("@SellerCode", decryptedSupplierCode);
            cmdForSeller.Parameters.AddWithValue("@OrderID", OrderID);
            SqlDataAdapter adapter = new SqlDataAdapter(cmdForSeller);
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            DataTable dt = ds.Tables[0];
            DataTable dt1 = ds.Tables[1];
            DataTable dt2 = ds.Tables[2];
            con.Close();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                invoice.InvoiceNumber = dt.Rows[i]["InvoiceNumber"].ToString();
                invoice.GenerateDate = dt.Rows[i]["generateDate"].ToString();
                invoice.DeliveryDate = dt.Rows[i]["generateDate"].ToString();
                invoice.TotalPrice = Convert.ToSingle(dt.Rows[i]["TotalPrice"]);
                invoice.SellerName = dt.Rows[i]["SellerName"].ToString();
                invoice.SellerCompanyName = dt.Rows[i]["SellerCompanyName"].ToString();
                invoice.SellerAddress = dt.Rows[i]["SellerAddress"].ToString();
                invoice.SellerPhone = dt.Rows[i]["SellerPhone"].ToString();
            }
            for (int i = 0; i < dt1.Rows.Count; i++)
            {
                invoice.OrderNo = dt1.Rows[i]["OrderNo"].ToString();
                invoice.OrderDate = dt1.Rows[i]["OrderDate"].ToString();
                invoice.BuyerName = dt1.Rows[i]["BuyerName"].ToString();
                invoice.BuyerAddress = dt1.Rows[i]["BuyerAddress"].ToString();
                invoice.BuyerPhone = dt1.Rows[i]["BuyerPhone"].ToString();
            }
            for (int i = 0; i < dt2.Rows.Count; i++)
            {
                ProductDetails product = new ProductDetails
                {
                    ProductName = dt2.Rows[i]["ProductName"].ToString(),
                   
                    Specification = dt2.Rows[i]["Specification"].ToString(),
                    Quantity = Convert.ToInt32(dt2.Rows[i]["Quantity"]),
                    Price = Convert.ToSingle(dt2.Rows[i]["Price"]),
                    Discount = Convert.ToSingle(dt2.Rows[i]["Discount"]),
                    DeliveryCharge = Convert.ToSingle(dt2.Rows[i]["DeliveryCharge"]),
                    SubTotalPrice = Convert.ToSingle(dt2.Rows[i]["SubTotalPrice"])
                };
                // Add the ProductDetails object to the productDetailsList
                invoice.ProductDetailsList.Add(product);
            }
            return Ok(new { message = "Sellers Order Invoice got successfully", invoice });
        }



    }
}
