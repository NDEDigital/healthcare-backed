using Microsoft.AspNetCore.Mvc;
using NDE_Digital_Market.Model;
using NDE_Digital_Market.SharedServices;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using NDE_Digital_Market.Model.MaterialStock;

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
        private CommonServices _commonServices;
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

        //[HttpPost, Authorize(Roles = "seller")]
        [HttpPost]
        [Route("AddProduct")]
        public IActionResult AddProcuct([FromForm] GoodsQuantityModel product)
        {

            product.AddedDate = DateTime.Now;
            product.UpdatedDate = DateTime.Now;

            string decryptedSupplierCode = CommonServices.DecryptPassword(product.SellerCode);
            product.SellerCode = decryptedSupplierCode;
            //string rootPath = _hostingEnvironment.ContentRootPath;
            //Console.WriteLine(rootPath);
            //string path = Path.Combine(rootPath,@"images\Uploads", product.ImageName);
            string path = Path.Combine(@"E:\Nimpex Health Care\NDE-Digital-Medical-Front-\src\assets\images\Uploads", product.ImageName);
            //string path = Path.Combine(@"C:\NDE-Digital-Market\dist\nde-digital-market\assets\images\Uploads", product.ImageName);
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

           
            string query = @"INSERT INTO ProductList
    (GoodsName, GroupCode, GroupName, Specification, Price, SellerCode, Quantity, 
     QuantityUnit, AddedDate, UpdatedDate, AddedBy, UpdatedBy, AddedPc, UpdatedPc, 
     ImagePath, Status)
VALUES
    (@GoodsName, @GroupCode, @GroupName, @Specification, @Price, @SellerCode, @Quantity, 
     @QuantityUnit, @AddedDate, @UpdatedDate, @AddedBy, @UpdatedBy, @AddedPc, @UpdatedPc, 
     @ImagePath, @Status);
 SELECT SCOPE_IDENTITY();
";


            SqlCommand cmd = new SqlCommand(query, con);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@GoodsName", product.GoodsName);
            cmd.Parameters.AddWithValue("@GroupCode", product.GroupCode);
            cmd.Parameters.AddWithValue("@GroupName", product.GroupName);
            cmd.Parameters.AddWithValue("@Specification", product.Specification);
            cmd.Parameters.AddWithValue("@Price", product.Price);
            cmd.Parameters.AddWithValue("@SellerCode", product.SellerCode);
            cmd.Parameters.AddWithValue("@Quantity", product.quantity);
            cmd.Parameters.AddWithValue("@QuantityUnit", product.QuantityUnit);
            cmd.Parameters.AddWithValue("@AddedDate", DateTime.Now); // or product.AddedDate if it's already set
            cmd.Parameters.AddWithValue("@UpdatedDate", DateTime.Now); // or product.UpdatedDate if it's already set
            cmd.Parameters.AddWithValue("@AddedBy", product.AddedBy);
            cmd.Parameters.AddWithValue("@UpdatedBy", product.UpdatedBy);
            cmd.Parameters.AddWithValue("@AddedPc", product.AddedPc);
            cmd.Parameters.AddWithValue("@UpdatedPc", product.UpdatedPc);
            cmd.Parameters.AddWithValue("@ImagePath", product.ImagePath);
            cmd.Parameters.AddWithValue("@Status", product.Status);

            try
            {
                con.Open();
                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    product.GoodsId = result.ToString();
                }
                con.Close();
                List<MaterialStock> stock = new List<MaterialStock>();
                MaterialStock obj = new MaterialStock();
                obj.GroupCode = product.GroupCode;
                obj.GoodsId = Convert.ToInt32(product.GoodsId);
                obj.SellerCode = product.SellerCode;
                obj.SalesQty = 0;
                obj.OperationType = "ADD";
                stock.Add(obj);
                string ans = _commonServices.InsertUpdateStockQt(stock);

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
            
                if (phoneNumber == "admin")
                {
                    isAdmin = true;
                }
            }
            con.Close();
            List<GoodsQuantityModel> Products = new List<GoodsQuantityModel>();

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
                    modelObj.AddedDate = Convert.ToDateTime(dt1.Rows[i]["AddedDate"]);      
                    Products.Add(modelObj);
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
        public List<GoodsQuantityModel>GetSellerProduct(string sellerCode)
        {
            Console.WriteLine(sellerCode, "sellerCode");
            string decryptedSupplierCode = CommonServices.DecryptPassword(sellerCode);

            string query = @"SELECT 
                            ProductList.GoodsId, 
                            ProductList.GoodsName, 
                            ProductList.GroupCode,
                            ProductList.GroupName,
                            ProductList.Specification,
                            ProductList.Price,
                            ProductList.SellerCode,
                            ProductList.ImagePath,
                            ISNULL(MaterialStockQty.PresentQty, 0) AS Quantity,
                            ProductList.QuantityUnit,  
	                        UserRegistration.CompanyName

                         FROM ProductList
                        LEFT JOIN
                        UserRegistration
                        ON
                        ProductList.SellerCode = UserRegistration.UserCode
                        LEFT JOIN
                                                MaterialStockQty
                                                ON
                                                   MaterialStockQty.GroupCode = ProductList.GroupCode AND MaterialStockQty.GoodsId = ProductList.GoodsId
                        WHERE ProductList.SellerCode = @DecryptedSupplierCode  ORDER BY ProductList.UpdatedDate DESC; ";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@DecryptedSupplierCode", decryptedSupplierCode);
          
            con.Open();
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();

            adapter.Fill(dt);

            con.Close();

            List<GoodsQuantityModel> sellerProducts = new List<GoodsQuantityModel>();

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

                sellerProducts.Add(modelObj);

            }
            return sellerProducts;

        }

        // ======================= DELETE Product ==================

        [HttpDelete, Authorize(Roles = "seller")]
        [Route("DeleteProduct")]
        public IActionResult DeleteProcuct(string sellerCode, int ProductId)
        {
            string decryptedSupplierCode = CommonServices.DecryptPassword(sellerCode);


            SqlCommand cmd = new SqlCommand("DELETE FROM ProductList WHERE  GoodsId = @ProductId AND SellerCode = @Aw", con);

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
