using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using NDE_Digital_Market.DTOs;
using NDE_Digital_Market.Model;
using NDE_Digital_Market.SharedServices;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;

namespace NDE_Digital_Market.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewAndRatingController : ControllerBase
    {
        private readonly string foldername;
        private readonly IConfiguration _configuration;
        private readonly SqlConnection con;
        //private readonly string foldername = "D:/HealthCare/healthcare-frontend/src/assets/images/Productfiles";
        private readonly string filename = "Reviewfile";
        public ReviewAndRatingController(IConfiguration configuration)
        {
            _configuration = configuration;
            CommonServices commonServices = new CommonServices(_configuration);
            con = new SqlConnection(commonServices.HealthCareConnection);
            foldername = commonServices.FilesPath + "Reviewfiles";
        }

        //[HttpPost ]
        //[Route("getReviewRatingsData")]

        //public IActionResult getReviewRatingsData([FromForm] int GoodsId, [FromForm] string GroupCode)
        //{
        //    int totalRating = 5;  // total rating 
        //    int Rating1, Rating2, Rating3, Rating4, Rating5 ,totalCount = 0;
        //    int[] ratingsArray = new int[totalRating];
        //    SqlConnection con = new SqlConnection(_connectionDigitalMarket);
        //    List<ReviewsAndRatings> reviewsAndRatings = new List<ReviewsAndRatings>();

        //    con.Open();
        //    SqlCommand cmdForReviews = new SqlCommand("GetReviewAndRetingsData", con);
        //    cmdForReviews.CommandType = CommandType.StoredProcedure;
        //    cmdForReviews.Parameters.AddWithValue("@GroupCode", GroupCode);
        //    cmdForReviews.Parameters.AddWithValue("@GoodsId", GoodsId);
        //    SqlDataAdapter adapter = new SqlDataAdapter(cmdForReviews);
        //    DataSet ds = new DataSet();
        //    adapter.Fill(ds);
        //    DataTable ratingCount = ds.Tables[0];
        //    DataTable Reviews = ds.Tables[1];
        //    con.Close();
        //    //for( int i=0; i < ratingCount.Rows.Count; i++)
        //    //{
        //    //    Rating1 = Convert.ToInt32(ratingCount.Rows[i].ToString());
        //    //    Rating2 = Convert.ToInt32(ratingCount.Rows[i].ToString());
        //    //    Rating3 = Convert.ToInt32(ratingCount.Rows[i].ToString());
        //    //    Rating4 = Convert.ToInt32(ratingCount.Rows[i].ToString());
        //    //    Rating5 = Convert.ToInt32(ratingCount.Rows[i].ToString());
        //    //    totalCount = Convert.ToInt32(ratingCount.Rows[i].ToString());

        //    //}

        //    if (ratingCount.Rows.Count > 0)
        //    {
        //        DataRow ratingRow = ratingCount.Rows[0];
        //        ratingsArray[4] = Convert.ToInt32(ratingRow["CountRating1"]);
        //        ratingsArray[3] = Convert.ToInt32(ratingRow["CountRating2"]);
        //        ratingsArray[2] = Convert.ToInt32(ratingRow["CountRating3"]);
        //        ratingsArray[1] = Convert.ToInt32(ratingRow["CountRating4"]);
        //        ratingsArray[0] = Convert.ToInt32(ratingRow["CountRating5"]);
        //        totalCount = Convert.ToInt32(ratingRow["TotalCount"]);
        //    }

        //    for (int i = 0; i < Reviews.Rows.Count; i++)
        //    {
        //        ReviewsAndRatings reviews = new ReviewsAndRatings();
        //        {
        //            reviews.ReviewId = Convert.ToInt32(Reviews.Rows[i]["ReviewId"]);
        //            reviews.GoodsId = Convert.ToInt32(Reviews.Rows[i]["GoodsId"]);
        //            reviews.OrderDetailId = Convert.ToInt32(Reviews.Rows[i]["OrderDetailId"]);
        //            reviews.BuyerId =  Reviews.Rows[i]["BuyerId"].ToString() ;   
        //            reviews.BuyerName = Reviews.Rows[i]["BuyerName"].ToString();
        //            reviews.SellerId = Reviews.Rows[i]["SellerId"].ToString();
        //            reviews.SellerName = Reviews.Rows[i]["SellerName"].ToString();
        //            reviews.GroupName = Reviews.Rows[i]["GroupName"].ToString();
        //            reviews.GroupCode = Reviews.Rows[i]["GroupCode"].ToString();
        //            reviews.DateTime = Convert.ToDateTime(Reviews.Rows[i]["DateTime"]);
        //            reviews.ReviewText = Reviews.Rows[i]["ReviewText"].ToString();
        //            reviews.RatingValue = Convert.ToInt32(Reviews.Rows[i]["RatingValue"]);
        //            reviews.ImagePath = Reviews.Rows[i]["ImagePath"].ToString();
        //            int emptyRating = totalRating - reviews.RatingValue ?? 0;
        //            int ratingValue = reviews.RatingValue ?? 0;
        //            int[] emptyRatingArray = Enumerable.Range(1, emptyRating).ToArray(); // creating empty array
        //            reviews.EmptyRatingArray = JsonConvert.SerializeObject(emptyRatingArray);
        //            int[] ratingArray = Enumerable.Range(1, ratingValue).ToArray();     // creating array of rating value
        //            reviews.RatingArray = JsonConvert.SerializeObject(ratingArray);
        //            reviews.BuyerCode = CommonServices.EncryptPassword(reviews.BuyerId);
        //        }
        //        reviewsAndRatings.Add(reviews);

        //    }




        //    return Ok(new { message = "Sellers ", reviewsAndRatings , ratingsArray, totalCount });
        //}


        [HttpPost]
        [Route("getReviewRatingsData")]
        public async Task<IActionResult> AddReview([FromForm] ReviewsAndRatings review)
        {
            string ImagePath = string.Empty;

            if (review.ImageFile != null)
            {
                ImagePath = CommonServices.UploadFiles(foldername, filename, review.ImageFile);
                review.ImagePath = ImagePath;
            }

            using (con)
            {
                string query = @"
    INSERT INTO ReviewRatings
        (OrderDetailId, ReviewText, RatingValue, BuyerId, ProductGroupID, ProductId, 
         SellerId, ReviewDate, ImagePath, AddedDate, AddedBy, AddedPc)
    VALUES
        (@OrderDetailId, @ReviewText, @RatingValue, @BuyerId, @ProductGroupID, @ProductId, 
         @SellerId, @ReviewDate, @ImagePath, @AddedDate, @AddedBy, @AddedPc);";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@OrderDetailId", review.OrderDetailId);
                    cmd.Parameters.AddWithValue("@ReviewText", review.ReviewText ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@RatingValue", review.RatingValue ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@BuyerId", review.BuyerId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ProductGroupID", review.ProductGroupID ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ProductId", review.ProductId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SellerId", review.SellerId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ReviewDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@ImagePath", ImagePath ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@AddedDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@AddedBy", review.AddedBy ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@AddedPc", review.AddedPc ?? (object)DBNull.Value);
                    try
                    {
                        await con.OpenAsync();
                        int a = await cmd.ExecuteNonQueryAsync();

                        if (a > 0)
                        {
                            SqlCommand command = new SqlCommand("UPDATE OrderDetails SET Status = 'Reviewed' WHERE OrderDetailId = " + review.OrderDetailId + "", con);

                            int updateResult = await command.ExecuteNonQueryAsync();

                            if (updateResult <= 0)
                            {

                                
                                return BadRequest(new { message = "product review status isn't change or not found." });
                            }
                        }
                        else
                        {
                            return BadRequest(new { message = "product review data isn't Inserted Successfully." });
                        }

                        return Ok(new { message = "Review added successfully." });

                    }
                    catch (Exception ex)
                    {

                        return StatusCode(500, "Error adding review: " + ex.Message);
                    }
                }
            }
        }


        //private async Task<string> SaveImage(IFormFile imageFile)
        //{

        //}







        //[HttpPost, Authorize(Roles = "buyer")]
        //    [Route("addReviewAndRating")]
        //    public async Task<IActionResult> Post([FromForm] ReviewsAndRatings reviewAndRating)
        //    {
        //        // Guid GeneratingRandomNumber = Guid.NewGuid();
        //        //Console.WriteLine(newGuid.ToString());
        //        string decryptedBuyerCode = CommonServices.DecryptPassword(reviewAndRating.BuyerId);
        //        if(reviewAndRating.ImageName!= null && reviewAndRating.Image != null)
        //        {
        //            //string path = Path.Combine(@"C:\development\NDE Medical\NDE-Digital-Medical-Front-\src\assets\images\Uploads\Review", reviewAndRating.ImageName);
        //            //string path = Path.Combine(@"C:\NDE-Digital-Market\dist\nde-digital-market\assets\images\Uploads\Review", reviewAndRating.ImageName);
        //            string path = Path.Combine(@"E:\Nimpex Health Care\NDE-Digital-Medical-Front-\src\assets\images\Uploads", reviewAndRating.ImageName);
        //            reviewAndRating.ImagePath = path;
        //            using (Stream stream = new FileStream(path, FileMode.Create))
        //            {
        //                reviewAndRating.Image.CopyTo(stream);
        //            }

        //        }


        //        try
        //        {
        //            // Perform input validation
        //            if (!ModelState.IsValid)
        //            {
        //                return BadRequest(ModelState);
        //            }


        //            using (SqlConnection connection = new SqlConnection(_connectionDigitalMarket))
        //            {
        //                await connection.OpenAsync();

        //                // Wrap the operations in a transaction
        //                using (var transaction = connection.BeginTransaction())
        //                {
        //                    try
        //                    {
        //                        string sqlQuery = "INSERT INTO ReviewRatings (OrderDetailId, ReviewText, RatingValue, BuyerId, GroupCode, GoodsId, GroupName, SellerId, DateTime, ImagePath) " +
        //                            "VALUES (@OrderDetailsId, @ReviewText, @RatingValue, @BuyerId, @GroupCode, @GoodsId, @GroupName, @SellerId, @DateTime, @ImagePath)" +
        //                            " UPDATE OrderDetails SET Status='Reviewed' Where OrderDetailId = @OrderDetailsId ";


        //                        using (SqlCommand command = new SqlCommand(sqlQuery, connection, transaction))
        //                        {
        //                            command.Parameters.AddWithValue("@OrderDetailsId", reviewAndRating.OrderDetailId);

        //                            command.Parameters.AddWithValue("@ReviewText", (reviewAndRating.ReviewText != null) ? reviewAndRating.ReviewText : " ");
        //                            command.Parameters.AddWithValue("@RatingValue", reviewAndRating.RatingValue);
        //                            command.Parameters.AddWithValue("@BuyerId", decryptedBuyerCode);
        //                            command.Parameters.AddWithValue("@GroupCode", reviewAndRating.GroupCode);
        //                            command.Parameters.AddWithValue("@GoodsId", reviewAndRating.GoodsId);
        //                            command.Parameters.AddWithValue("@GroupName", reviewAndRating.GroupName ?? "");
        //                            command.Parameters.AddWithValue("@SellerId", reviewAndRating.SellerId);
        //                            command.Parameters.AddWithValue("@DateTime", DateTime.Now);
        //                            if (reviewAndRating.ImagePath != null)
        //                            {
        //                                command.Parameters.AddWithValue("@ImagePath", reviewAndRating.ImagePath);
        //                            }
        //                           else command.Parameters.AddWithValue("@ImagePath"," ");

        //                            await command.ExecuteNonQueryAsync();
        //                        }
        
        //                        // Commit the transaction if everything is successful
        //                        transaction.Commit();

        //                        return Ok(new { message = "Review and rating added successfully." });
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        // Rollback the transaction in case of an exception
        //                        transaction.Rollback();

        //                        // Log the exception
        //                        // ...

        //                        // Return an appropriate error response
        //                        return StatusCode(StatusCodes.Status500InternalServerError, "Error adding review or rating.");
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            // Handle exceptions, log errors, or return an appropriate error response
        //            return StatusCode(StatusCodes.Status500InternalServerError, "Error adding review or rating: " + ex.Message);
        //        }
        //    }
        //    //Delete Review
        //    [HttpDelete ]
        //    [Route("DeleteReview")]
        //    public IActionResult DeleteProcuct(int ReviewId)
        //    {
        //        SqlConnection con = new SqlConnection(_connectionDigitalMarket);
        //        SqlCommand cmd = new SqlCommand("DELETE FROM ReviewRatings WHERE ReviewId = @ReviewId", con);

        //        cmd.CommandType = CommandType.Text;
        //        cmd.Parameters.AddWithValue("@ReviewId", ReviewId);
        //        con.Open();
        //        cmd.ExecuteNonQuery();
        //        con.Close();

        //        return Ok(new { message = "Review DELETED successfully" });
        //    }
        // ======================= Update Product ==================

        //[HttpPut]
        //[Route("UpdateReviewAndRatings")]
        //public IActionResult UpdateReviewAndRatings(int reviewId, int rating, String? review = null)
        //{
        //    SqlConnection con = new SqlConnection(_connectionSteel);
        //    SqlCommand cmd = new SqlCommand("UPDATE ReviewRatings SET RatingValue =@RatingValue ,ReviewText =@ReviewText   WHERE ReviewId = @ReviewId", con);
        //    cmd.CommandType = CommandType.Text;
        //    cmd.Parameters.AddWithValue("@ReviewId", reviewId);
        //    cmd.Parameters.AddWithValue("@RatingValue", rating);
        //    cmd.Parameters.AddWithValue("@ReviewText", (review != null) ? review : " ");
        //    con.Open();
        //    cmd.ExecuteNonQuery();
        //    con.Close();
        //    return Ok(new { message = "Review updated successfully" });

        //}



        //[HttpPut, Authorize(Roles = "buyer")]
        //[Route("UpdateReviewAndRatings")]
        //public IActionResult UpdateReviewAndRatings([FromForm] ReviewsAndRatings reviewAndRating)
        //{
        //    SqlConnection con = new SqlConnection(_connectionDigitalMarket);
        //    SqlCommand cmd = new SqlCommand("UPDATE ReviewRatings SET RatingValue =@RatingValue ,ReviewText =@ReviewText   WHERE ReviewId = @ReviewId", con);
        //    cmd.CommandType = CommandType.Text;
        //    cmd.Parameters.AddWithValue("@ReviewId", reviewAndRating.ReviewId);
        //    cmd.Parameters.AddWithValue("@RatingValue", reviewAndRating.RatingValue);
        //    cmd.Parameters.AddWithValue("@ReviewText", (reviewAndRating.ReviewText != null) ? reviewAndRating.ReviewText : " ");
        //    con.Open();
        //    cmd.ExecuteNonQuery();
        //    con.Close();
        //    return Ok(new { message = "Review updated successfully" });

        //}
    }
}
