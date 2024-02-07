using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NDE_Digital_Market.DTOs;
using NDE_Digital_Market.SharedServices;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace NDE_Digital_Market.Controllers
{
    public class DashboardGetData : Controller
    {
        private readonly string _healthCareConnection;

        public DashboardGetData(IConfiguration config)
        {
            CommonServices commonServices = new CommonServices(config);
            _healthCareConnection = commonServices.HealthCareConnection;
        }

        [HttpGet]
        [Route("SellerPermissionData/{UserId}/{Status1}")]
        public async Task<IActionResult> GetPermissionData(int UserId, int Status1)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_healthCareConnection))
                {
                    string query = @"
                        DECLARE @Status INT = @Status1;
                        DECLARE @UserId INT = @UserId1;

                        IF @Status = 0
                        BEGIN
                            SELECT P.UserId, P.MenuId, M.IsActive, M.MenuName 
                            FROM Permission P 
                            JOIN MenuList M ON P.MenuId = M.MenuId
                            WHERE P.UserId = @UserId AND M.IsActive = 1;
                        END
                        ELSE IF @Status = 1
                        BEGIN
                            SELECT MenuId, MenuName
                            FROM MenuList
                            WHERE IsAdmin != 1 AND IsActive = 1;
                        END
                    ";

                    await con.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@UserId1", UserId);
                        cmd.Parameters.AddWithValue("@Status1", Status1); // Fix: Use @Status instead of status

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            var result = new List<PermissionToDashDto>();
                            if(Status1 == 1)
                            {
                                while (reader.Read())
                                {
                                    var permission = new PermissionToDashDto
                                    {
                                        MenuId = Convert.ToInt32(reader["MenuId"]),
                                        MenuName = reader["MenuName"].ToString(),
                                        // Add other properties if needed
                                    };

                                    result.Add(permission);
                                }
                            }
                            else
                            {
                                while (reader.Read())
                                {
                                    var permission = new PermissionToDashDto
                                    {
                                        UserId = Convert.ToInt32(reader["UserId"]),
                                        MenuId = Convert.ToInt32(reader["MenuId"]),
                                        MenuName = reader["MenuName"].ToString(),
                                        // Add other properties if needed
                                    };

                                    result.Add(permission);
                                }
                            }


                            return Ok(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
