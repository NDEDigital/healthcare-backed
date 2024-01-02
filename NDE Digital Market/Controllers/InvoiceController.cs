using Microsoft.AspNetCore.Mvc;
using NDE_Digital_Market.Model.OrderModel;
using NDE_Digital_Market.SharedServices;
using System.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using NDE_Digital_Market.DTOs;
using NDE_Digital_Market.Model;

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

        [HttpGet]
        [Route("GetInvoiceDataForBuyer")]
        public async Task<IActionResult> GetInvoiceDataForBuyer(int OrderMasterId)
        {
            GetOrderInvoiceByMasterIdDto invoice = new GetOrderInvoiceByMasterIdDto();
            SqlConnection con = new SqlConnection(connectionHealthCare);
            string queryForBuyer = "GetOrderInvoiceByMasterId";
            con.Open();
            SqlCommand cmdForBuyer = new SqlCommand(queryForBuyer, con);
            cmdForBuyer.CommandType = CommandType.StoredProcedure;

            cmdForBuyer.Parameters.AddWithValue("@OrderMasterId", OrderMasterId);
            SqlDataAdapter adapter = new SqlDataAdapter(cmdForBuyer);
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            DataTable reader = ds.Tables[0];
            DataTable reader1 = ds.Tables[1];
            con.Close();
            for (int i = 0; i < reader.Rows.Count; i++)
            {
                invoice.InvoiceNumber = reader.Rows[i]["InvoiceNumber"].ToString();
                invoice.OrderDate = Convert.ToDateTime(reader.Rows[i]["OrderDate"].ToString());
                invoice.BuyerName = reader.Rows[i]["BuyerName"].ToString();
                invoice.Address = reader.Rows[i]["Address"].ToString();
                invoice.Phone = reader.Rows[i]["Phone"].ToString();
                invoice.PaymentMethod = reader.Rows[i]["PaymentMethod"].ToString();
                invoice.NumberOfItem = Convert.ToInt32(reader.Rows[i]["NumberOfItem"].ToString());
                invoice.TotalPrice = Convert.ToDecimal(reader.Rows[i]["TotalPrice"].ToString());
 
                //invoice.OrderInvoiceDetailList.Add(buyerdata);
            }
            for (int i = 0; i < reader1.Rows.Count; i++)
            {
                OrderInvoiceDetails orderDetails = new OrderInvoiceDetails
                {
                    ProductName = reader1.Rows[i]["ProductName"].ToString(),
                    Specification = reader1.Rows[i]["Specification"].ToString(),
                    Quantity = Convert.ToInt32(reader1.Rows[i]["Quantity"].ToString()),
                    Unit = reader1.Rows[i]["Unit"].ToString(),
                    Price = Convert.ToDecimal(reader1.Rows[i]["Price"].ToString()),
                    DeliveryCharge = Convert.ToDecimal(reader1.Rows[i]["DeliveryCharge"].ToString()),
                    DiscountAmount = Convert.ToDecimal(reader1.Rows[i]["DiscountAmount"].ToString()),
                    DiscountPct = Convert.ToDecimal(reader1.Rows[i]["DiscountPct"].ToString()),
                    NetPrice = Convert.ToDecimal(reader1.Rows[i]["NetPrice"].ToString()),
                    DetailDeliveryCharge = Convert.ToDecimal(reader1.Rows[i]["DetailDeliveryCharge"].ToString()),
                    SubTotalPrice = Convert.ToDecimal(reader1.Rows[i]["SubTotalPrice"].ToString()),
                    SelesPerson = reader1.Rows[i]["SelesPerson"].ToString(),
                    SelesAddress = reader1.Rows[i]["SelesAddress"].ToString(),
                    SellerContact = reader1.Rows[i]["SellerContact"].ToString(),
                    Company = reader1.Rows[i]["Company"].ToString(),
                };
                invoice.OrderInvoiceDetailList.Add(orderDetails); 
            }
            return Ok(new { message = "Buyer Order Invoice got successfully", invoice });
        }


        //========================================== Added By Rey =================================
        // Get Invoice data For Seller

        //[HttpGet]
        //[Route("GetInvoiceDataForSeller")]
        //public async Task<IActionResult> GetInvoiceDataForSeller(int SSMId)
        //{
        //    try
        //    {
        //List<SellerInvoice> objectlist = new List<SellerInvoice>();

        //        using (SqlConnection con = new SqlConnection(connectionHealthCare))
        //        {
        //            string query = "SellerInvoice";

        //            SqlCommand sqlCommand = new SqlCommand(query, con);

        //            sqlCommand.CommandType = CommandType.StoredProcedure;
        //            sqlCommand.Parameters.AddWithValue("@SSMId", SSMId);

        //            await con.OpenAsync();

        //            SqlDataReader reader = await sqlCommand.ExecuteReaderAsync();

        //            if (!reader.HasRows)
        //            {
        //                return BadRequest(new { message = "No Order Data Found." });
        //            }

        //            while (await reader.ReadAsync())
        //            {
        //                SellerInvoice details = new SellerInvoice();
        //                {
        //                    details.SSMId = Convert.ToInt32(reader["SSMId"].ToString());
        //                    details.SSMCode = reader["SSMCode"].ToString();
        //                    details.SSMDate = Convert.ToDateTime(reader["SSMDate"].ToString());
        //                    details.CompanyCode = reader["CompanyCode"].ToString();
        //                    details.CompanyName = reader["CompanyName"].ToString();
        //                    details.TotalPrice = Convert.ToDecimal(reader["TotalPrice"].ToString());
        //                    details.Challan = reader["Challan"].ToString();
        //                    details.TotalPrice = Convert.ToDecimal(reader["TotalPrice"].ToString());
        //                    details.Remarks = reader["Remarks"].ToString();
        //                    details.BUserId = Convert.ToInt32(reader["BUserId"].ToString());
        //                    details.BuyerName = reader["BuyerName"].ToString();
        //                    details.OrderNo = reader["OrderNo"] == DBNull.Value ? null : reader["OrderNo"].ToString();
        //                    details.ProductId = Convert.ToInt32(reader["ProductId"].ToString());
        //                    details.ProductName = reader["ProductName"].ToString();
        //                    details.Specification = reader["Specification"].ToString();


        //                    details.StockQty = Convert.ToDecimal(reader["StockQty"].ToString());
        //                    details.SaleQty = Convert.ToInt32(reader["SaleQty"].ToString());

        //                    details.UnitId = Convert.ToInt32(reader["UnitId"].ToString());
        //                    details.Unit = reader["Unit"].ToString();


        //                    details.NetPrice = Convert.ToDecimal(reader["NetPrice"].ToString());


        //                    details.SSLRemarks = reader["SSLRemarks"].ToString();


        //                    details.Address = reader["Address"].ToString();

        //                    details.ProductGroupID = Convert.ToInt32(reader["ProductGroupID"].ToString());
        //                    details.ProductGroupName = reader["ProductGroupName"].ToString();
        //                }

        //                objectlist.Add(details);
        //            }
        //            await con.CloseAsync();
        //        }

        //        return Ok(objectlist);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //}

        [HttpGet]
        [Route("GetInvoiceDataForSeller")]
        public async Task<IActionResult> GetInvoiceDataForSeller( int SSMId)
        {
            SellerInvoice invoice = new SellerInvoice();
            SqlConnection con = new SqlConnection(connectionHealthCare);
            string queryForSeller = "SellerInvoice";
            con.Open();
            SqlCommand cmdForSeller = new SqlCommand(queryForSeller, con);
            cmdForSeller.CommandType = CommandType.StoredProcedure;
            cmdForSeller.Parameters.AddWithValue("@SSMId", SSMId);
            SqlDataAdapter adapter = new SqlDataAdapter(cmdForSeller);
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            DataTable reader = ds.Tables[0];
            DataTable reader1 = ds.Tables[1];
            con.Close();
            for (int i = 0; i < reader.Rows.Count; i++)
            {


                invoice.SSMCode = reader.Rows[i]["SSMCode"].ToString();
                invoice.SSMDate = Convert.ToDateTime(reader.Rows[i]["SSMDate"].ToString());
                invoice.SelesPerson = reader.Rows[i]["SelesPerson"].ToString();
                invoice.Company = reader.Rows[i]["Company"].ToString();
                invoice.SelesAddress = reader.Rows[i]["SelesAddress"].ToString();
                invoice.Phone = reader.Rows[i]["Phone"].ToString();
                invoice.Challan = reader.Rows[i]["Challan"].ToString();
                invoice.Remarks = reader.Rows[i]["Remarks"].ToString();
            }

            for (int i = 0; i < reader1.Rows.Count; i++)
            {

                SellerInvoiceDetails sellerDetails = new SellerInvoiceDetails
                {
                    OrderNo = reader1.Rows[i]["OrderNo"].ToString(),
                    ProductGroupName = reader1.Rows[i]["ProductGroupName"].ToString(),
                    ProductName = reader1.Rows[i]["ProductName"].ToString(),
                    Specification = reader1.Rows[i]["Specification"].ToString(),
                    StockQty = Convert.ToDecimal(reader1.Rows[i]["StockQty"].ToString()),
                    SaleQty = Convert.ToInt32(reader1.Rows[i]["SaleQty"].ToString()),
                    Unit = reader1.Rows[i]["Unit"].ToString(),
                    NetPrice = Convert.ToDecimal(reader1.Rows[i]["NetPrice"].ToString()),
                    SSLRemarks = reader1.Rows[i]["SSLRemarks"].ToString(),
                    BuyerName = reader1.Rows[i]["BuyerName"].ToString(),
                    BuyerPhone = reader1.Rows[i]["BuyerPhone"].ToString(),
                    Address = reader1.Rows[i]["Address"].ToString(),

                };
                // Add the ProductDetails object to the productDetailsList
                invoice.SellerInvoiceDetailList.Add(sellerDetails);
            }
            return Ok(new { message = "Sellers Order Invoice got successfully", invoice });
        }


        /////==============================================================================================
        //[HttpGet, Authorize(Roles = "seller")]
        //[Route("GetInvoiceDataForSeller")]
        //public IActionResult GetInvoiceDataForSeller(string sellerCode, int OrderID)
        //{
        //    string decryptedSupplierCode = CommonServices.DecryptPassword(sellerCode);
        //    SellerInvoiceModel invoice = new SellerInvoiceModel();
        //    SqlConnection con = new SqlConnection(_connectionDigitalMarket);
        //    string queryForSeller = "sp_InvoiceDataForSeller";
        //    con.Open();
        //    SqlCommand cmdForSeller = new SqlCommand(queryForSeller, con);
        //    cmdForSeller.CommandType = CommandType.StoredProcedure;
        //    cmdForSeller.Parameters.AddWithValue("@SellerCode", decryptedSupplierCode);
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
        //        invoice.InvoiceNumber = dt.Rows[i]["InvoiceNumber"].ToString();
        //        invoice.GenerateDate = dt.Rows[i]["generateDate"].ToString();
        //        invoice.DeliveryDate = dt.Rows[i]["generateDate"].ToString();
        //        invoice.TotalPrice = Convert.ToSingle(dt.Rows[i]["TotalPrice"]);
        //        invoice.SellerName = dt.Rows[i]["SellerName"].ToString();
        //        invoice.SellerCompanyName = dt.Rows[i]["SellerCompanyName"].ToString();
        //        invoice.SellerAddress = dt.Rows[i]["SellerAddress"].ToString();
        //        invoice.SellerPhone = dt.Rows[i]["SellerPhone"].ToString();
        //    }
        //    for (int i = 0; i < dt1.Rows.Count; i++)
        //    {
        //        invoice.OrderNo = dt1.Rows[i]["OrderNo"].ToString();
        //        invoice.OrderDate = dt1.Rows[i]["OrderDate"].ToString();
        //        invoice.BuyerName = dt1.Rows[i]["BuyerName"].ToString();
        //        invoice.BuyerAddress = dt1.Rows[i]["BuyerAddress"].ToString();
        //        invoice.BuyerPhone = dt1.Rows[i]["BuyerPhone"].ToString();
        //    }
        //    for (int i = 0; i < dt2.Rows.Count; i++)
        //    {
        //        ProductDetails product = new ProductDetails
        //        {
        //            ProductName = dt2.Rows[i]["ProductName"].ToString(),

        //            Specification = dt2.Rows[i]["Specification"].ToString(),
        //            Quantity = Convert.ToInt32(dt2.Rows[i]["Quantity"]),
        //            Price = Convert.ToSingle(dt2.Rows[i]["Price"]),
        //            Discount = Convert.ToSingle(dt2.Rows[i]["Discount"]),
        //            DeliveryCharge = Convert.ToSingle(dt2.Rows[i]["DeliveryCharge"]),
        //            SubTotalPrice = Convert.ToSingle(dt2.Rows[i]["SubTotalPrice"])
        //        };
        //        // Add the ProductDetails object to the productDetailsList
        //        invoice.ProductDetailsList.Add(product);
        //    }
        //    return Ok(new { message = "Sellers Order Invoice got successfully", invoice });
        //}



    }
}
