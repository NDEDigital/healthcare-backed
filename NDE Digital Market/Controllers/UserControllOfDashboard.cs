using Microsoft.AspNetCore.Mvc;
using NDE_Digital_Market.SharedServices;
using System.Data.SqlClient;

public class UserControllOfDashboard : ControllerBase
{
    private readonly string _healthCareConnection;

    public UserControllOfDashboard(IConfiguration config)
    {
        CommonServices commonServices = new CommonServices(config);
        _healthCareConnection = commonServices.HealthCareConnection;
    }

    [HttpDelete("deleteMenuItems/{UserId}")]
    public async Task<IActionResult> DeleteMenuItems(int UserId, [FromBody] List<int> menuIdsToDelete)
    {
        try
        {
            using (SqlConnection con = new SqlConnection(_healthCareConnection))
            {
                await con.OpenAsync();

                // Create a parameterized query with dynamic number of parameters
                string query = $"DELETE FROM Permission WHERE UserId = @UserId AND MenuId IN ({string.Join(",", menuIdsToDelete.Select((id, index) => $"@MenuId{index}"))})";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", UserId);

                    // Add parameters for each menu ID
                    for (int i = 0; i < menuIdsToDelete.Count; i++)
                    {
                        cmd.Parameters.AddWithValue($"@MenuId{i}", menuIdsToDelete[i]);
                    }

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    if (rowsAffected > 0)
                    {
                        return Ok(new { message = "Menu items deleted successfully." });
                    }
                    else
                    {
                        return NotFound(new { message = "User or menu items not found." });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log the exception
            // logger.LogError(ex, "An error occurred while processing the request.");
            return StatusCode(500, $"An error occurred while processing the request. Details: {ex.Message}");
        }
    }

}
