using Microsoft.AspNetCore.Mvc;
using NDE_Digital_Market.DTOs;
using NDE_Digital_Market.SharedServices;
using Newtonsoft.Json;
using System.Data.SqlClient;

namespace NDE_Digital_Market.Controllers
{
    public class SellerActive_Inactive : Controller
    {
        private readonly string _healthCareConnection;

        public SellerActive_Inactive(IConfiguration config)
        {
            CommonServices commonServices = new CommonServices(config);
            _healthCareConnection = commonServices.HealthCareConnection;
        }

        [HttpGet]
        [Route("getSellerActive&Inactive/{IsSeller}")]
        public List<sellerStatus> CompanySellerDetails(string CompanyCode, bool IsSeller, bool IsActive)
        {
            List<sellerStatus> bidList = new List<sellerStatus>();

            try
            {
                using (SqlConnection con = new SqlConnection(_healthCareConnection))
                {
                    con.Open();

                    using (SqlCommand cmd = new SqlCommand(@"DECLARE @SpecificCompanyCode NVARCHAR(255);

                                                        IF EXISTS (SELECT * FROM CompanyRegistration WHERE CompanyCode = @CompanyCode)
                                                            SET @SpecificCompanyCode = @CompanyCode;
                                                        ELSE
                                                            SET @SpecificCompanyCode = NULL;

                                                        SELECT
                                                            UR.UserId,
                                                            UR.FullName,
                                                            UR.PhoneNumber,
                                                            UR.Email,
                                                            UR.Address,
                                                            UR.AddedDate,
                                                            UR.IsActive,
                                                            UR.CompanyCode,
                                                            UR.IsBuyer,
                                                            UR.IsSeller,
                                                        
	                                                        CR.CompanyCode,
	                                                        CR.CompanyName,
                                                            CR.CompanyAdminId
                                                        FROM
                                                            UserRegistration UR
                                                        JOIN
                                                            CompanyRegistration CR ON UR.CompanyCode = CR.CompanyCode
                                                        WHERE
                                                            (
                                                                (UR.IsSeller = @IsSeller AND UR.IsActive = @IsActive AND
                                                                    (UR.CompanyCode = @SpecificCompanyCode OR @SpecificCompanyCode IS NULL)
                                                                )
                                                            )
                                                            AND
                                                            (
                                                                CR.CompanyCode = @SpecificCompanyCode OR @SpecificCompanyCode IS NULL
                                                            );


                                                                    ", con))
                    {
                        cmd.Parameters.AddWithValue("@CompanyCode", CompanyCode);
                        cmd.Parameters.AddWithValue("@IsActive", IsActive);

                        cmd.Parameters.AddWithValue("@IsSeller", IsSeller);



                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                sellerStatus bid = new sellerStatus();
                                bid.UserId = Convert.ToInt32(reader["UserId"]);
                                //UserId = reader.GetInt32(userId),
                                bid.FullName = reader["FullName"].ToString();
                                bid.PhoneNumber = reader["PhoneNumber"].ToString();
                                bid.Email = reader["Email"].ToString();
                                bid.Address = reader["Address"].ToString();
                                bid.AddedDate = (DateTime)(reader["AddedDate"] as DateTime?);
                                bid.IsActive = reader["IsActive"] as bool? ?? IsActive;
                                bid.IsSeller = reader["IsSeller"] as bool? ?? IsActive;


                                bid.CompanyCode = reader["CompanyCode"].ToString();
                                bid.CompanyAdminId = Convert.ToInt32(reader["CompanyAdminId"]);

                                bid.CompanyName = reader["CompanyName"].ToString();


                                bidList.Add(bid);
                            }
                        }
                    }

                    con.Close();



                }



                return bidList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                // You might want to handle errors more gracefully
                return null;
            }
        }
        [HttpPut]
        [Route("updateSellerActive&Inactive")]
        public async Task<IActionResult> UpdateSellerProductStatusAsync(string userIds, bool isActive)
        {
            try
            {
                string query = $" UPDATE UserRegistration SET IsActive = @IsActive WHERE UserId IN ({userIds})";

                using (var connection = new SqlConnection(_healthCareConnection))
                {

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        await connection.OpenAsync();
                        command.Parameters.AddWithValue("@IsActive", isActive);
                        await command.ExecuteNonQueryAsync();
                        await connection.CloseAsync();
                    }


                }

                return Ok(new { message = "updated seller" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }
    }
}
