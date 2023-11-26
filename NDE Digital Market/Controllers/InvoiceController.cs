using Microsoft.AspNetCore.Mvc;
using NDE_Digital_Market.Model.OrderModel;
using NDE_Digital_Market.SharedServices;
using System.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Authorization;

namespace NDE_Digital_Market.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : Controller
    {

        private readonly string _connectionSteel;
     
        private readonly string _connectionDigitalMarket;
        public InvoiceController(IConfiguration config)
        {
            _connectionSteel = config.GetConnectionString("DefaultConnection");
            
            _connectionDigitalMarket = config.GetConnectionString("DigitalMarketConnection");
        }

        //========================================== Added By Maru =================================
        // Get Invoice data For Admin
        [HttpGet, Authorize(Roles = "admin")]
        [Route("GetInvoiceDataForAdmin")]
        public IActionResult GetInvoiceDataForAdmin(int OrderID)
        {

            AdminOrderInVoiceModel invoice = new AdminOrderInVoiceModel();
            SqlConnection con = new SqlConnection(_connectionDigitalMarket);
            string queryForAdmin = "sp_InvoiceForAdmin";
            con.Open();
            SqlCommand cmdForSeller = new SqlCommand(queryForAdmin, con);
            cmdForSeller.CommandType = CommandType.StoredProcedure;

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
                AdminSellerDetails sellerdata = new AdminSellerDetails
                {
                    InvoiceNumber = dt.Rows[i]["InvoiceNumber"].ToString(),
                    GenerateDate = dt.Rows[i]["generateDate"].ToString(),
                    DeliveryDate = dt.Rows[i]["generateDate"].ToString(),

                    SellerName = dt.Rows[i]["SellerName"].ToString(),
                    SellerCompanyName = dt.Rows[i]["SellerCompanyName"].ToString(),
                    SellerAddress = dt.Rows[i]["SellerAddress"].ToString(),
                    SellerPhone = dt.Rows[i]["SellerPhone"].ToString(),
                    SellerCode = dt.Rows[i]["SellerCode"].ToString(),


                };
                invoice.SellerDetailsList.Add(sellerdata);
            }
            for (int i = 0; i < dt1.Rows.Count; i++)
            {
                invoice.OrderNo = dt1.Rows[i]["OrderNo"].ToString();
                invoice.OrderDate = dt1.Rows[i]["OrderDate"].ToString();
                invoice.BuyerName = dt1.Rows[i]["BuyerName"].ToString();
                invoice.TotalPrice = Convert.ToSingle(dt1.Rows[i]["TotalPrice"]).ToString();
                invoice.BuyerAddress = dt1.Rows[i]["BuyerAddress"].ToString();
                invoice.BuyerPhone = dt1.Rows[i]["BuyerPhone"].ToString();
            }
            for (int i = 0; i < dt2.Rows.Count; i++)
            {
                AdminProductDetails product = new AdminProductDetails
                {
                    ProductName = dt2.Rows[i]["ProductName"].ToString(),
                    Specification = dt2.Rows[i]["Specification"].ToString(),
                    SellerCode = dt2.Rows[i]["SellerCode"].ToString(),
                    Quantity = Convert.ToInt32(dt2.Rows[i]["Quantity"]),
                    Price = Convert.ToSingle(dt2.Rows[i]["Price"]),
                    Discount = Convert.ToSingle(dt2.Rows[i]["Discount"]),
                    DeliveryCharge = Convert.ToSingle(dt2.Rows[i]["DeliveryCharge"]),
                    SubTotalPrice = Convert.ToSingle(dt2.Rows[i]["SubTotalPrice"]),
                    //       OrderDetailId = Convert.ToInt32(dt.Rows[i]["OrderDetailId"]),
                };
                // Add the ProductDetails object to the productDetailsList
                invoice.ProductDetailsList.Add(product);
            }
            return Ok(new { message = "Sellers Order Invoice got successfully", invoice });
        }

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
