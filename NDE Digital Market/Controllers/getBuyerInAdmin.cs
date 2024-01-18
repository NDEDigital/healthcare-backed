
using Microsoft.AspNetCore.Mvc;
using NDE_Digital_Market.DTOs;
using NDE_Digital_Market.SharedServices;
using System.Data.SqlClient;

namespace NDE_Digital_Market.Controllers
{
    public class getBuyerInAdmin : Controller
    {
        private readonly string _healthCareConnection;

        public getBuyerInAdmin(IConfiguration config)
        {
            CommonServices commonServices = new CommonServices(config);
            _healthCareConnection = commonServices.HealthCareConnection;
        }

        [HttpGet]
        [Route("getBuyerInAdmin/{IsBuyer}")]
        public List<sellerStatus> CompanySellerDetails( bool IsBuyer, bool IsActive)
        {
            List<sellerStatus> bidList = new List<sellerStatus>();

            try
            {
                using (SqlConnection con = new SqlConnection(_healthCareConnection))
                {
                    con.Open();

                    using (SqlCommand cmd = new SqlCommand(@"SELECT
                                                UR.UserId,
                                                UR.FullName,
                                                UR.PhoneNumber,
                                                UR.Email,
                                                UR.Address,
                                                UR.AddedDate,
                                                UR.IsActive,
                                                UR.CompanyCode,
	                                            UR.IsBuyer
                                            FROM
                                                UserRegistration UR
                                            WHERE
   
                                                (UR.IsBuyer = 1 AND UR.IsActive = @IsActive); 
                                                                    ", con))
                    {
                       
                        cmd.Parameters.AddWithValue("@IsActive", IsActive);

                        cmd.Parameters.AddWithValue("@IsBuyer", IsBuyer);


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
                                bid.IsBuyer = reader["IsBuyer"] as bool? ?? IsBuyer;
                             


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
       
        
    }
}

