using Microsoft.AspNetCore.Mvc;
using NDE_Digital_Market.DTOs;
using NDE_Digital_Market.SharedServices;
using System.Data.SqlClient;
using System.Security.Cryptography;

namespace NDE_Digital_Market.Controllers
{
    public class Dashboard : Controller
    {
        private readonly string _healthCareConnection;

        public Dashboard(IConfiguration config)
        {
            CommonServices commonServices = new CommonServices(config);
            _healthCareConnection = commonServices.HealthCareConnection;
        }

     

        [HttpGet]
        [Route("sellerDashboard/{UserId}")]
        public List<DashboardDto> CompanySellerDetails(string UserId)
        {
            List<DashboardDto> dbList = new List<DashboardDto>();

            try
            {
                using (SqlConnection con = new SqlConnection(_healthCareConnection))
                {
                    con.Open();

                    using (SqlCommand cmd = new SqlCommand(@" SELECT M.MenuId,M.MenuName,M.IsActive
                                                FROM MenuList M
                                                LEFT JOIN Permission P ON M.MenuId = P.MenuId AND P.UserId = @UserId
                                                WHERE M.IsActive=1 AND P.MenuId IS NULL AND M.IsAdmin!=1  ;
", con))
                    {

                        cmd.Parameters.AddWithValue("@UserId", UserId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DashboardDto db = new DashboardDto();
                                db.MenuId = Convert.ToInt32(reader["MenuId"]);
                                //UserId = reader.GetInt32(userId),
                                db.MenuName = reader["MenuName"].ToString();


                              






                                dbList.Add(db);

                            }
                        }
                    }

                    con.Close();



                }



                return dbList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                // You might want to handle errors more gracefully
                return null;
            }
        }



    }
}
