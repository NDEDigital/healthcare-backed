using Microsoft.AspNetCore.Mvc;
using NDE_Digital_Market.Model;
using NDE_Digital_Market.SharedServices;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Microsoft.AspNetCore.Authorization;


namespace NDE_Digital_Market.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly SqlConnection con;
        //private readonly IWebHostEnvironment _hostingEnvironment;
        //public ProductsController(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        public ProductsController(IConfiguration configuration)
        {
            _configuration = configuration;
            //_hostingEnvironment = hostingEnvironment;
            con = new SqlConnection(_configuration.GetConnectionString("DigitalMarketConnection"));
            //string rootPath = _hostingEnvironment.ContentRootPath;
            //Console.WriteLine(rootPath);
        }


        //===================================== Create User ================================

        //By => "User"
        //data => DateTime.Now
        //PC => "0.0.0.0"

        [HttpPost, Authorize(Roles = "seller")]
        [Route("AddProduct")]
        public IActionResult AddProcuct([FromForm] ProductsModel product)
        {

            product.AddedDate = DateTime.Now;
            product.UpdateDate = DateTime.Now;

            string decryptedSupplierCode = CommonServices.DecryptPassword(product.SupplierCode);
            product.SupplierCode = decryptedSupplierCode;
            //string rootPath = _hostingEnvironment.ContentRootPath;
            //Console.WriteLine(rootPath);
            //string path = Path.Combine(rootPath,@"images\Uploads", product.ImageName);
           //string path = Path.Combine(@"G:\NDEDigitalMarket\src\assets\images\Uploads", product.ImageName);
            string path = Path.Combine(@"C:\NDE-Digital-Market\dist\nde-digital-market\assets\images\Uploads", product.ImageName);
            product.ImagePath = path;
            //Console.WriteLine(path);
            product.AddedBy = decryptedSupplierCode;
            product.UpdatedBy = decryptedSupplierCode;

            using (Stream stream = new FileStream(path, FileMode.Create))
            {
                product.Image.CopyTo(stream);
            }
            string systemCode = string.Empty;

            SqlCommand cmdSP = new SqlCommand("spMakeSystemCode", con);
            {
                cmdSP.CommandType = CommandType.StoredProcedure;
                cmdSP.Parameters.AddWithValue("@TableName", "ProductList");
                cmdSP.Parameters.AddWithValue("@Date", DateTime.Now.ToString("yyyy-MM-dd"));
                cmdSP.Parameters.AddWithValue("@AddNumber", 1);

                con.Open();
                systemCode = cmdSP.ExecuteScalar()?.ToString();
                con.Close();
            }

            product.ProductId = int.Parse(systemCode.Split('%')[0]);
            product.ProductCode = systemCode.Split('%')[1];
            SqlCommand cmd = new SqlCommand("INSERT INTO ProductList VALUES  (@ProductId,@ProductCode,@ProductName,@ProductDescription," +
                "@MaterialType,@MaterialName,@Height,@Width,@Length,@Weight,@Finish,@Grade,@Price,@ImagePath,@SupplierCode,@Quantity,@QuantityUnit,@DimensionUnit," +
                "@WeightUnit,@AddedDate,@UpdatedDate,@AddedBy,@UpdatedBy,@AddedPC,@UpdatedPC,@Status,@StatusBit)", con);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@ProductId", product.ProductId);
            cmd.Parameters.AddWithValue("@ProductCode", product.ProductCode);
            cmd.Parameters.AddWithValue("@ProductName", product.ProductName);
            cmd.Parameters.AddWithValue("@ProductDescription", product.ProductDescription);
            cmd.Parameters.AddWithValue("@MaterialType", product.MaterialType);
            cmd.Parameters.AddWithValue("@MaterialName", product.MaterialName);
            cmd.Parameters.AddWithValue("@Height", product.Height);
            cmd.Parameters.AddWithValue("@Width", product.Width);
            cmd.Parameters.AddWithValue("@Length", product.Length);
            cmd.Parameters.AddWithValue("@Weight", product.Weight);
            cmd.Parameters.AddWithValue("@Finish", product.Finish);
            cmd.Parameters.AddWithValue("@Grade", product.Grade);
            cmd.Parameters.AddWithValue("@Price", product.Price);
            cmd.Parameters.AddWithValue("@ImagePath", product.ImagePath);
            cmd.Parameters.AddWithValue("@SupplierCode", decryptedSupplierCode);
            cmd.Parameters.AddWithValue("@Quantity", product.Quantity);
            cmd.Parameters.AddWithValue("@QuantityUnit", product.QuantityUnit);
            cmd.Parameters.AddWithValue("@DimensionUnit", product.DimensionUnit);
            cmd.Parameters.AddWithValue("@WeightUnit", product.WeightUnit);
            cmd.Parameters.AddWithValue("@AddedDate", product.AddedDate);
            cmd.Parameters.AddWithValue("@UpdatedDate", product.UpdateDate);
            cmd.Parameters.AddWithValue("@AddedBy", product.AddedBy);
            cmd.Parameters.AddWithValue("@UpdatedBy", product.UpdatedBy);
            cmd.Parameters.AddWithValue("@AddedPC", product.AddedPC);
            cmd.Parameters.AddWithValue("@UpdatedPC", product.UpdatedPC);
            cmd.Parameters.AddWithValue("@Status", product.Status);
            cmd.Parameters.AddWithValue("@StatusBit", product.StatusBit);

            try
            {
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
                return Ok(); // If execution is successful
            }
            catch (SqlException ex)
            {
                // Handle any SQL-related errors
                return BadRequest("SQL Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                // Handle any other errors
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }


        }

        [HttpPut, Authorize(Roles = "seller")]
        [Route("UpdateProduct")]

        public IActionResult UpdateProduct([FromForm] ProductsModel product)
        {
            string decryptedSupplierCode = CommonServices.DecryptPassword(product.SupplierCode);
            product.UpdatedBy = decryptedSupplierCode;
            product.UpdateDate = DateTime.Now;
            //SqlCommand cmd = new SqlCommand("UPDATE ProductList SET ProductName = @ProductName,ProductDescription = @ProductDescription," +
            //    "MaterialType = @MaterialType,MaterialName=@MaterialName,Height = @Height,Width = @Width,Length = @Length,Weight = @Weight,Finish = @Finish,Grade = @Grade,Price = @Price,Quantity = @Quantity," +
            //    "QuantityUnit = @QuantityUnit,DimensionUnit = @DimensionUnit, WeightUnit = @WeightUnit,UpdatedDate = @UpdateDate,UpdatedBy = @UpdatedBy,UpdatedPC = @UpdatedPC,Status=@Status, StatusBit=@StatusBit" +
            //    " WHERE  SupplierCode = @SupplierCode AND ProductId = @ProductId", con);
            SqlCommand cmd = new SqlCommand("INSERT INTO EditedProductList (ProductId,ProductName, ProductDescription, MaterialType, MaterialName, Height, Width, Length, Weight, Finish, Grade, Price, Quantity, QuantityUnit, DimensionUnit, WeightUnit, UpdatedDate, UpdatedBy, UpdatedPC, Status, StatusBit, SupplierCode)VALUES(@ProductId,@ProductName, @ProductDescription, @MaterialType, @MaterialName, @Height, @Width, @Length, @Weight, @Finish, @Grade, @Price, @Quantity, @QuantityUnit, @DimensionUnit, @WeightUnit, @UpdateDate, @UpdatedBy, @UpdatedPC, @Status, @StatusBit, @SupplierCode);" +
                " UPDATE ProductList SET status=@Status,StatusBit=4 WHERE  SupplierCode = @SupplierCode AND ProductId = @ProductId", con);
            cmd.CommandType = CommandType.Text;


            cmd.Parameters.AddWithValue("@ProductName", product.ProductName);
            cmd.Parameters.AddWithValue("@ProductDescription", product.ProductDescription);
            cmd.Parameters.AddWithValue("@MaterialType", product.MaterialType);
            cmd.Parameters.AddWithValue("@MaterialName", product.MaterialName);
            cmd.Parameters.AddWithValue("@Height", product.Height);
            cmd.Parameters.AddWithValue("@Width", product.Width);
            cmd.Parameters.AddWithValue("@Length", product.Length);
            cmd.Parameters.AddWithValue("@Weight", product.Weight);
            cmd.Parameters.AddWithValue("@Finish", product.Finish);
            cmd.Parameters.AddWithValue("@Grade", product.Grade);
            cmd.Parameters.AddWithValue("@Price", product.Price);

            cmd.Parameters.AddWithValue("@Quantity", product.Quantity);
            cmd.Parameters.AddWithValue("@QuantityUnit", product.QuantityUnit);
            cmd.Parameters.AddWithValue("@DimensionUnit", product.DimensionUnit);
            cmd.Parameters.AddWithValue("@WeightUnit", product.WeightUnit);
            cmd.Parameters.AddWithValue("@UpdateDate", product.UpdateDate);
            cmd.Parameters.AddWithValue("@UpdatedBy", product.UpdatedBy);
            cmd.Parameters.AddWithValue("@UpdatedPC", product.UpdatedPC);
            cmd.Parameters.AddWithValue("@Status", product.Status);
            cmd.Parameters.AddWithValue("@StatusBit", product.StatusBit);
            //
            cmd.Parameters.AddWithValue("@SupplierCode", decryptedSupplierCode);
            cmd.Parameters.AddWithValue("@ProductId", product.ProductId);

            //con.Open();
            //cmd.ExecuteNonQuery();
            //con.Close();
            try
            {
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
                return Ok(); // If execution is successful
            }
            catch (SqlException ex)
            {
                // Handle any SQL-related errors
                return BadRequest("SQL Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                // Handle any other errors
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
           

        }

       
        
        // ======================= GET Dashboard Contents ================== 

        [HttpGet ]
        [Route("GetDashboardContents")]

        //public Tuple<List<SellerProductsModel>, object> GetDashboardContents(string sellerCode, String? status = null)
        public IActionResult GetDashboardContents(string sellerCode, String? status = null, String? productName = null, String? companyName = null, DateTime? addedDate = null)
        {
            Console.WriteLine(sellerCode, "sellerCode");
            string decryptedSupplierCode = CommonServices.DecryptPassword(sellerCode);

            bool isAdmin = false;
            int newCount = 0, editedCount = 0, approvedCount = 0, rejectedCount = 0;
            string query = "SELECT PhoneNumber FROM UserRegistration WHERE UserCode = @UserCode;";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@UserCode", decryptedSupplierCode);
            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                string phoneNumber = reader["PhoneNumber"].ToString();
                con.Close();
                if (phoneNumber == "admin")
                {
                    isAdmin = true;
                }
            }

            List<SellerProductsModel> Products = new List<SellerProductsModel>();

            if (isAdmin && status != null)
            {

                Console.WriteLine(isAdmin);
                Console.WriteLine("isAdmin");


                string queryForAdmin = "sp_ProductListWithCompanyName";

                SqlCommand cmdForAdmin = new SqlCommand(queryForAdmin, con);
                cmdForAdmin.CommandType = CommandType.StoredProcedure;
                cmdForAdmin.Parameters.AddWithValue("@Status", status);
                if (productName != null) { cmdForAdmin.Parameters.AddWithValue("@productName", productName); }
                if (companyName != null) { cmdForAdmin.Parameters.AddWithValue("@CompanyName", companyName); }
                if (addedDate != null) { cmdForAdmin.Parameters.AddWithValue("@AddedDate", addedDate); }
                SqlDataAdapter adapter = new SqlDataAdapter(cmdForAdmin);
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                DataTable dt = ds.Tables[0];
                DataTable dt1 = ds.Tables[1];

                con.Close();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    newCount = Convert.ToInt32(dt.Rows[i]["NewCount"]);
                    editedCount = Convert.ToInt32(dt.Rows[i]["EditedCount"]);
                    approvedCount = Convert.ToInt32(dt.Rows[i]["ApprovedCount"]);
                    rejectedCount = Convert.ToInt32(dt.Rows[i]["RejectedCount"]);

                }

                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    SellerProductsModel obj = new SellerProductsModel();
                    obj.ProductId = Convert.ToInt32(dt1.Rows[i]["ProductId"]);
                    obj.Status = dt1.Rows[i]["Status"].ToString();
                    obj.StatusBit = Convert.ToInt32(dt1.Rows[i]["StatusBit"]);
                    obj.ProductCode = dt1.Rows[i]["ProductCode"].ToString();
                    obj.ProductName = dt1.Rows[i]["ProductName"].ToString();
                    obj.ProductDescription = dt1.Rows[i]["ProductDescription"].ToString();
                    obj.MaterialType = dt1.Rows[i]["MaterialType"].ToString();
                    obj.MaterialName = dt1.Rows[i]["MaterialName"].ToString();
                    obj.Height = Convert.ToSingle(dt1.Rows[i]["Height"]);
                    obj.Width = Convert.ToSingle(dt1.Rows[i]["Width"]);
                    obj.Length = Convert.ToSingle(dt1.Rows[i]["Length"]);
                    obj.Weight = Convert.ToSingle(dt1.Rows[i]["Weight"]);
                    obj.Finish = dt1.Rows[i]["Finish"].ToString();
                    obj.Grade = dt1.Rows[i]["Grade"].ToString();
                    obj.Price = Convert.ToSingle(dt1.Rows[i]["Price"]);
                    obj.ImagePath = dt1.Rows[i]["ImagePath"].ToString();
                    obj.SupplierCode = dt1.Rows[i]["SupplierCode"].ToString();
                    obj.Quantity = Convert.ToSingle(dt1.Rows[i]["Quantity"]);
                    obj.QuantityUnit = dt1.Rows[i]["QuantityUnit"].ToString();
                    obj.DimensionUnit = dt1.Rows[i]["DimensionUnit"].ToString();
                    obj.WeightUnit = dt1.Rows[i]["WeightUnit"].ToString();
                    obj.companyName = dt1.Rows[i]["CompanyName"].ToString();
                    obj.AddedDate = Convert.ToDateTime(dt1.Rows[i]["AddedDate"]);

                    //obj.IsAdmin = isAdmin;

                    Products.Add(obj);
                }
            }
            else
            {
                isAdmin = false;
                Products = GetSellerProduct(sellerCode);
            }

            //return Tuple.Create(Products, (object)isAdmin);
            return Ok(new { message = "content get successfully", Products, isAdmin, newCount, editedCount, approvedCount, rejectedCount });



        }

        // ======================= GET Product ==================

        [HttpGet]
        [Route("GetProduct")]
        public List<SellerProductsModel> GetSellerProduct(string sellerCode)
        {
            Console.WriteLine(sellerCode, "sellerCode");
            string decryptedSupplierCode = CommonServices.DecryptPassword(sellerCode);

            string query = "SELECT * FROM ProductList WHERE SupplierCode = @DecryptedSupplierCode AND Status='Approved' ORDER BY UpdatedDate DESC;";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@DecryptedSupplierCode", decryptedSupplierCode);
            con.Close();
            con.Open();
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();

            adapter.Fill(dt);

            con.Close();

            List<SellerProductsModel> sellerProducts = new List<SellerProductsModel>();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                SellerProductsModel obj = new SellerProductsModel();


                obj.ProductId = Convert.ToInt32(dt.Rows[i]["ProductId"]);
                obj.Status = dt.Rows[i]["Status"].ToString();
                obj.StatusBit = Convert.ToInt32(dt.Rows[i]["StatusBit"]);
                obj.ProductCode = dt.Rows[i]["ProductCode"].ToString();
                obj.ProductName = dt.Rows[i]["ProductName"].ToString();
                obj.ProductDescription = dt.Rows[i]["ProductDescription"].ToString();
                obj.MaterialType = dt.Rows[i]["MaterialType"].ToString();
                obj.MaterialName = dt.Rows[i]["MaterialName"].ToString();
                obj.Height = Convert.ToSingle(dt.Rows[i]["Height"]);
                obj.Width = Convert.ToSingle(dt.Rows[i]["Width"]);
                obj.Length = Convert.ToSingle(dt.Rows[i]["Length"]);
                obj.Weight = Convert.ToSingle(dt.Rows[i]["Weight"]);
                obj.Finish = dt.Rows[i]["Finish"].ToString();
                obj.Grade = dt.Rows[i]["Grade"].ToString();
                obj.Price = Convert.ToSingle(dt.Rows[i]["Price"]);
                obj.ImagePath = dt.Rows[i]["ImagePath"].ToString();
                obj.SupplierCode = dt.Rows[i]["SupplierCode"].ToString();
                obj.Quantity = Convert.ToSingle(dt.Rows[i]["Quantity"]);
                obj.QuantityUnit = dt.Rows[i]["QuantityUnit"].ToString();
                obj.DimensionUnit = dt.Rows[i]["DimensionUnit"].ToString();
                obj.WeightUnit = dt.Rows[i]["WeightUnit"].ToString();

                sellerProducts.Add(obj);

            }
            return sellerProducts;

        }

        // ======================= DELETE Product ==================

        [HttpDelete, Authorize(Roles = "seller")]
        [Route("DeleteProduct")]
        public IActionResult DeleteProcuct(string sellerCode, int ProductId)
        {
            string decryptedSupplierCode = CommonServices.DecryptPassword(sellerCode);


            SqlCommand cmd = new SqlCommand("DELETE FROM ProductList WHERE  ProductId = @ProductId AND SupplierCode = @SupplierCode", con);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@ProductId", ProductId);
            cmd.Parameters.AddWithValue("@SupplierCode", decryptedSupplierCode);
            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();
            return Ok(new { message = "Product DELETED successfully" });


        }

        // ======================= Update Product Status ==================
        //[HttpPut]
        //[Route("UpdateProductStatus")]

        //public IActionResult UpdateProductStatus([FromForm] UpdateProductStatusModel Obj)
        //{
        //    Console.WriteLine(Obj);
        //    string decryptedSupplierCode = CommonServices.DecryptPassword(Obj.userCode);
        //    string UpdatedBy = decryptedSupplierCode;
        //    DateTime UpdateDate = DateTime.Now;
        //    Console.WriteLine(Obj.productIDs);
        //    int statusBit=1;
        //    if (Obj.status == "approved")
        //    {
        //         statusBit = 2;
        //    }
        //    if(Obj.status == "rejected"){ statusBit = 3; }
        //    string query = $"UPDATE ProductList  SET Status = '{Obj.status}',StatusBit= '{statusBit}', UpdatedBy = '{UpdatedBy}', UpdatedDate = '{UpdateDate}',updatedPC= '{Obj.updatedPC}'  WHERE ProductID IN ({Obj.productIDs})";
        //    SqlCommand cmd = new SqlCommand(query,con);
        //    con.Open();
        //    cmd.CommandType = CommandType.Text;
        //    cmd.ExecuteNonQuery();
        //    con.Close();
        //    return Ok(new { message = "Product status updated successfully" });
        //}


        [HttpPut]
        [Route("UpdateProductStatus")]
        public IActionResult UpdateProductStatus([FromForm] UpdateProductStatusModel Obj)
        {
            Console.WriteLine(Obj);
            string decryptedSupplierCode = CommonServices.DecryptPassword(Obj.userCode);
            bool cancelEdited = false;
            List<EditedUserInfoModel> users = new List<EditedUserInfoModel>();
            string UpdatedBy = decryptedSupplierCode;
            DateTime UpdateDate = DateTime.Now;
            //Console.WriteLine(Obj.productIDs);
            int statusBit = 1;
            if (Obj.status == "approved")
            {
                statusBit = 2;
            }
            if (Obj.status == "rejected") 
            {
                statusBit = 3; 
            }

            if (Obj.statusBefore == "edited")
            {
                StringBuilder queryEdited = new StringBuilder();
                if (Obj.status == "rejected")
                {
                    cancelEdited = true;
                    queryEdited = queryEdited.Append($"UPDATE ProductList  SET Status = 'approved',StatusBit= 2, UpdatedBy = '{UpdatedBy}', UpdatedDate = '{UpdateDate}',UpdatedPC= '{Obj.updatedPC}'  WHERE ProductID IN ({Obj.productIDs})");
                    //queryEdited = queryEdited.Append($"SELECT SupplierCode,email,full_name FROM ProductList LEFT JOIN UserRegistration ON SupplierCode=UserCode WHERE ProductID IN ({Obj.productIDs})");
                }
                else
                {
                    // Split the product IDs into an array

                    string[] productIDsArray = Obj.productIDs.Split(',');

                    // Loop through each product ID
                    foreach (string productId in productIDsArray)
                    {

                        queryEdited.Append($"UPDATE ProductList SET ProductName = EPL.ProductName, ProductDescription = EPL.ProductDescription, " +
                        $"MaterialType = EPL.MaterialType,MaterialName = EPL.MaterialName,Height = EPL.Height,Length = EPL.Length,Weight = EPL.Weight," +
                        $"Finish = EPL.Finish,Grade = EPL.Grade,Price = EPL.Price,Quantity = EPL.Quantity,QuantityUnit" +
                        $" = EPL.QuantityUnit,DimensionUnit = EPL.DimensionUnit,WeightUnit = EPL.WeightUnit, ");

                        queryEdited.Append($"Status = '{Obj.status}', StatusBit = '{statusBit}', UpdatedBy = '{UpdatedBy}', UpdatedDate = '{UpdateDate}',updatedPC= '{Obj.updatedPC}'");
                        queryEdited.Append($"FROM (SELECT * FROM EditedProductList WHERE ProductID = {productId}) AS EPL ");
                        queryEdited.Append($"WHERE ProductList.ProductID = {productId} AND ProductList.SupplierCode = EPL.SupplierCode;");


                    }
                }

                StringBuilder deleteQuery = new StringBuilder();
                deleteQuery.Append($"DELETE FROM EditedProductList WHERE ProductID IN ({Obj.productIDs})");
                string combinedQuery = queryEdited.ToString() + deleteQuery.ToString();
                SqlCommand cmdEdite = new SqlCommand(combinedQuery, con);
                con.Open();
                cmdEdite.CommandType = CommandType.Text;
                cmdEdite.ExecuteNonQuery();

                string selectQuery = $"SELECT SupplierCode, Email, FullName,ProductName FROM ProductList LEFT JOIN UserRegistration ON SupplierCode=UserCode WHERE ProductID IN ({Obj.productIDs})";
                using (SqlCommand selectCmd = new SqlCommand(selectQuery, con))
                {
                    selectCmd.CommandType = CommandType.Text;

                    using (SqlDataReader reader = selectCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            EditedUserInfoModel user = new EditedUserInfoModel();
                            user.SupplierCode = reader["SupplierCode"].ToString();
                            user.Email = reader["Email"].ToString();
                            user.FullName = reader["FullName"].ToString();
                            user.ProductName = reader["ProductName"].ToString();
                            users.Add(user);
                        }
                        // Now you can use the 'users' list with the retrieved data.
                    }

                }
                con.Close();



            }
            else
            {
                string query = $"UPDATE ProductList  SET Status = '{Obj.status}',StatusBit= '{statusBit}', UpdatedBy = '{UpdatedBy}', UpdatedDate = '{UpdateDate}',UpdatedPC= '{Obj.updatedPC}'  WHERE ProductID IN ({Obj.productIDs})";
                SqlCommand cmd = new SqlCommand(query, con);

                con.Open();
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
                con.Close();
            }

            return Ok(new { message = "Product status updated successfully", cancelEdited, users });
        }

        public class EditedUserInfoModel
        {

            public string? FullName { get; set; }
            public string? SupplierCode { get; set; }
            public string? Email { get; set; }
            public string? ProductName { get; set; }
        }


        [HttpGet ]
        [Route("comapreEditedProduct")]
        public IActionResult comapreEditedProduct(int productId)
        {
            SellerProductsModel oldData = new SellerProductsModel();
            SellerProductsModel newData = new SellerProductsModel();
            string query = "SELECT * FROM ProductList WHERE ProductId=@ProductId; SELECT * FROM EditedProductList WHERE ProductId=@ProductId;";
            con.Open();
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@ProductId", productId);
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            DataTable dt0 = ds.Tables[0];
            DataTable dt1 = ds.Tables[1];
            con.Close();
            for (int i = 0; i < dt0.Rows.Count; i++)
            {
                oldData.ProductId = Convert.ToInt32(dt0.Rows[i]["ProductId"]);
                oldData.Status = dt0.Rows[i]["Status"].ToString();
                oldData.StatusBit = Convert.ToInt32(dt0.Rows[i]["StatusBit"]);
                oldData.ProductCode = dt0.Rows[i]["ProductCode"].ToString();
                oldData.ProductName = dt0.Rows[i]["ProductName"].ToString();
                oldData.ProductDescription = dt0.Rows[i]["ProductDescription"].ToString();
                oldData.MaterialType = dt0.Rows[i]["MaterialType"].ToString();
                oldData.MaterialName = dt0.Rows[i]["MaterialName"].ToString();
                oldData.Height = Convert.ToSingle(dt0.Rows[i]["Height"]);
                oldData.Width = Convert.ToSingle(dt0.Rows[i]["Width"]);
                oldData.Length = Convert.ToSingle(dt0.Rows[i]["Length"]);
                oldData.Weight = Convert.ToSingle(dt0.Rows[i]["Weight"]);
                oldData.Finish = dt0.Rows[i]["Finish"].ToString();
                oldData.Grade = dt0.Rows[i]["Grade"].ToString();
                oldData.Price = Convert.ToSingle(dt0.Rows[i]["Price"]);
                oldData.ImagePath = dt0.Rows[i]["ImagePath"].ToString();
                oldData.SupplierCode = dt0.Rows[i]["SupplierCode"].ToString();
                oldData.Quantity = Convert.ToSingle(dt0.Rows[i]["Quantity"]);
                oldData.QuantityUnit = dt0.Rows[i]["QuantityUnit"].ToString();
                oldData.DimensionUnit = dt0.Rows[i]["DimensionUnit"].ToString();
                oldData.WeightUnit = dt0.Rows[i]["WeightUnit"].ToString();
            }
            for (int i = 0; i < dt1.Rows.Count; i++)
            {
                newData.ProductId = Convert.ToInt32(dt1.Rows[i]["ProductId"]);
                newData.Status = dt1.Rows[i]["Status"].ToString();
                newData.StatusBit = Convert.ToInt32(dt1.Rows[i]["StatusBit"]);

                newData.ProductName = dt1.Rows[i]["ProductName"].ToString();
                newData.ProductDescription = dt1.Rows[i]["ProductDescription"].ToString();
                newData.MaterialType = dt1.Rows[i]["MaterialType"].ToString();
                newData.MaterialName = dt1.Rows[i]["MaterialName"].ToString();
                newData.Height = Convert.ToSingle(dt1.Rows[i]["Height"]);
                newData.Width = Convert.ToSingle(dt1.Rows[i]["Width"]);
                newData.Length = Convert.ToSingle(dt1.Rows[i]["Length"]);
                newData.Weight = Convert.ToSingle(dt1.Rows[i]["Weight"]);
                newData.Finish = dt1.Rows[i]["Finish"].ToString();
                newData.Grade = dt1.Rows[i]["Grade"].ToString();
                newData.Price = Convert.ToSingle(dt1.Rows[i]["Price"]);

                newData.SupplierCode = dt1.Rows[i]["SupplierCode"].ToString();
                newData.Quantity = Convert.ToSingle(dt1.Rows[i]["Quantity"]);
                newData.QuantityUnit = dt1.Rows[i]["QuantityUnit"].ToString();
                newData.DimensionUnit = dt1.Rows[i]["DimensionUnit"].ToString();
                newData.WeightUnit = dt1.Rows[i]["WeightUnit"].ToString();
            }
            return Ok(new { message = "GET Products data successful", oldData, newData });
        }
    }
}
