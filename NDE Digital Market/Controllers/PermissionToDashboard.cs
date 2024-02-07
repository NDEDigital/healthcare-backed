using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NDE_Digital_Market.DTOs;
using NDE_Digital_Market.SharedServices;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace NDE_Digital_Market.Controllers
{
    public class PermissionToDashboard : Controller
    {
        private readonly string _healthCareConnection;

        public PermissionToDashboard(IConfiguration config)
        {
            CommonServices commonServices = new CommonServices(config);
            _healthCareConnection = commonServices.HealthCareConnection;
        }

        [HttpPost]
        [Route("GiveAcessDashboard/{UserId}/{MenuId}")]
        public async Task<IActionResult> InsertPermissionToDashboard(int UserId, int MenuId)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_healthCareConnection))
                {
                    string query = @"INSERT INTO Permission (UserId, MenuId, IsActive, PermissionId) VALUES (@UserId,@MenuId, 1, (select Max(PermissionId) from Permission)+1);";

                    await con.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@UserId", UserId);
                        cmd.Parameters.AddWithValue("@MenuId", MenuId);


                        // ExecuteScalarAsync is used for queries that return a single value
                        var insertedItemId = await cmd.ExecuteScalarAsync();

                        // If needed, you can return the inserted ItemID
                        return Ok(new
                        {
                            Message = "item insert  successful",

                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
      [HttpGet]
[Route("GetPermissionData/{UserId}")]
public async Task<IActionResult> GetPermissionData(int UserId)
{
    try
    {
        using (SqlConnection con = new SqlConnection(_healthCareConnection))
        {
            string query = @"SELECT P.UserId,M.MenuId,P.PermissionId,M.MenuName,U.FullName,U.CompanyCode,U.FUllName
                       FROM Permission P 
                       JOIN MenuList M ON P.MenuId=M.MenuId
                       JOIN UserRegistration U ON P.UserId=U.UserId
                       WHERE M.IsAdmin!=1 AND M.IsActive=1 AND U.CompanyCode=(select CompanyCode from UserRegistration    WHERE UserId=@UserId) AND
                                        P.UserId!=@UserId AND P.UserId!=(select C.CompanyAdminId from UserRegistration U JOIN CompanyRegistration C ON U.CompanyCode=C.CompanyCode 
                                        WHERE UserId=@UserId) ;";

            await con.OpenAsync();

            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                // Add the @UserId parameter
                cmd.Parameters.AddWithValue("@UserId", UserId);

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    var result = new Dictionary<int, List<PermissionToDashDto>>();

                    while (reader.Read())
                    {
                        var userId = Convert.ToInt32(reader["UserId"]);

                        var permission = new PermissionToDashDto
                        {
                            UserId = userId,
                            MenuId = Convert.ToInt32(reader["MenuId"]),
                            MenuName = reader["MenuName"].ToString(),
                            FullName = reader["FullName"].ToString(),
                            PermissionId = reader.GetInt32(reader.GetOrdinal("PermissionId")),
                        };

                        if (!result.ContainsKey(userId))
                        {
                            result[userId] = new List<PermissionToDashDto>();
                        }

                        result[userId].Add(permission);
                    }

                    return Ok(result);
                }
            }
        }
    }
    catch (Exception ex)
    {
        // Handle exceptions appropriately (logging, returning an error response, etc.)
        return StatusCode(500, "Internal Server Error");
    }
}

    }
}

