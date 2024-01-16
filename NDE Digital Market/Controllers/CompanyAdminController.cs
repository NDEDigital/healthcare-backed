using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration; // Make sure to import this namespace
using NDE_Digital_Market.DTOs;
using NDE_Digital_Market.SharedServices;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace NDE_Digital_Market.Controllers
{
    public class CompanyAdminController : Controller
    {
        private readonly string _healthCareConnection;

        public CompanyAdminController(IConfiguration config)
        {
            CommonServices commonServices = new CommonServices(config);
            _healthCareConnection = commonServices.HealthCareConnection;
        }

        [HttpGet]
        [Route("CompanySellerDetails/{userId}/{IsActive}")]
        public List<CompanySellerList> CompanySellerDetails(int userId,bool IsActive)
        {
            List<CompanySellerList> bidList = new List<CompanySellerList>();

            try
            {
                using (SqlConnection con = new SqlConnection(_healthCareConnection))
                {
                    con.Open();

                    using (SqlCommand cmd = new SqlCommand(@"SELECT
                                            UR.UserId,
                                            UR.FullName,
                                            UR.PhoneNumber,
                                            CR.Email,
                                            UR.Address,
                                            UR.AddedDate,
                                            CR.IsActive,
                                            CR.CompanyCode,
CR.CompanyName
                                        FROM
                                            UserRegistration UR
                                        JOIN
                                            CompanyRegistration CR ON UR.CompanyCode = CR.CompanyCode
                                        WHERE
                                            CR.IsActive = 1
                                            AND UR.IsActive = @IsActive
                                            AND CR.CompanyAdminId = @UserId
                                            AND EXISTS (
                                                SELECT 1
                                                FROM UserRegistration
                                                WHERE CompanyCode = CR.CompanyCode AND UserId = @UserId
                                            );", con))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@IsActive", IsActive);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                CompanySellerList bid = new CompanySellerList
                                {
                                    UserId = Convert.ToInt32(reader["UserId"]),
                                    //UserId = reader.GetInt32(userId),
                                    FullName = reader["FullName"].ToString(),
                                    PhoneNumber = reader["PhoneNumber"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    Address = reader["Address"].ToString(),
                                    AddedDate = (DateTime)(reader["AddedDate"] as DateTime?),
                                    IsActive = reader["IsActive"] as bool? ?? IsActive,
                                    CompanyCode = reader["CompanyCode"].ToString(),
                                    CompanyName = reader["CompanyName"].ToString(),



                                };

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
        [Route("CompanySellerDetailsUpdateUserStatus/{userId}/{IsActive}")]
        public IActionResult UpdateUserStatus(int userId, bool IsActive)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_healthCareConnection))
                {
                    con.Open();

                    using (SqlCommand cmd = new SqlCommand(@"UPDATE UserRegistration
                                                     SET IsActive = @IsActive
                                                     WHERE UserId = @UserId;", con))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@IsActive", IsActive);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        Console.WriteLine(rowsAffected + "ekhane Jhamela");
                        if (rowsAffected > 0)
                        {
                            return Ok(new { message = "Updated Successfully" }); // Update successful
                        }
                        else
                        {
                            return BadRequest(); // User not found
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                // You might want to handle errors more gracefully
                return StatusCode(500, "Internal Server Error");
            }
        }

    }
}
