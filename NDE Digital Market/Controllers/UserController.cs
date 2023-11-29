﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NDE_Digital_Market.Model;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.Security.Cryptography;
using System.Text;
using NDE_Digital_Market.SharedServices;
using Microsoft.AspNetCore.Rewrite;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;

namespace NDE_Digital_Market.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionDigitalMarket;
        private readonly SqlConnection con;
        public UserController(IConfiguration configuration)
        {
            _configuration = configuration;
            con = new SqlConnection(_configuration.GetConnectionString("ProminentConnection"));
           // _connectionDigitalMarket = config.GetConnectionString("DigitalMarketConnection");
        }

        //===================================== Create User ================================
        [HttpPost]
        [Route("UserExist")]
        public IActionResult UserExist(UserModel user)
        {
            SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM  [UserRegistration] WHERE PhoneNumber = @phoneNumber", con);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@phoneNumber", user.PhoneNumber);
            con.Open();
            int count = (int)cmd.ExecuteScalar();
            con.Close();
            Boolean userExist = false;
            if (count > 0)
            {
                userExist = true;
            }
            return Ok(new { message = "User  exists check", userExist });
            //   return BadRequest(new { message = "User does not exist" , userExist });
        }
    
        [HttpPost]
        [Route("CreateUser")]
        public IActionResult CreateUser(UserModel user)
        {
            //SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM UserRegistration WHERE PhoneNumber = @phoneNumber", con);
            //cmd.CommandType = CommandType.Text;
            //cmd.Parameters.AddWithValue("@phoneNumber", user.PhoneNumber);
            //con.Open();
            //int count = (int)cmd.ExecuteScalar();
            //con.Close();

            //if (count > 0)
            //{
            //    return BadRequest(new { message = "User already exists" });
            //}
            //else
            //{
                //SP
                string systemCode = string.Empty;

                // Execute the stored procedure to generate the system code
                SqlCommand cmdSP = new SqlCommand("spMakeSystemCode", con);
                {
                    cmdSP.CommandType = CommandType.StoredProcedure;
                    cmdSP.Parameters.AddWithValue("@TableName", "UserRegistration");
                    cmdSP.Parameters.AddWithValue("@Date", DateTime.Now.ToString("yyyy-MM-dd"));
                    cmdSP.Parameters.AddWithValue("@AddNumber", 1);

                    //con.Open();
                    //using (SqlDataReader reader = cmdSP.ExecuteReader())
                    //{
                    //    if (reader.Read())
                    //    {
                    //        systemCode = reader["SystemCode"].ToString();
                    //    }
                    //}
                    //con.Close();
                    con.Open();
                    systemCode = cmdSP.ExecuteScalar()?.ToString();
                    con.Close();
                }

                user.UserID = int.Parse(systemCode.Split('%')[0]);
                user.UserCode = systemCode.Split('%')[1];
                //SP END

                // Encrypt the Password
            string encryptedPassword = CommonServices.EncryptPassword(user.Password);
            createPasswordHash(user.Password, out byte[] passwordHash, out byte[] passwordSalt);
            SqlCommand cmd = new SqlCommand("  INSERT INTO UserRegistration (UserId, UserCode, CountryRegion, IsBuyer, IsSeller, IsAdmin, FullName, PhoneNumber, Email, PasswordHash, PasswordSalt, Address, CompanyName, Website, ProductCategory, YearsInBusiness, BusinessRegistrationNumber, TaxIdNumber, PreferredPaymentMethod, TimeStamp)\nVALUES (@userID, @userCode, @contryRegion, @isBuyer, @isSeller, @isAdmin, @fullName, @phoneNumber, @email, @passwordHash, @passwordSalt, @address, @companyName, @website, @productCategory, @yearsInBusiness, @businessRegNum, @TaxIDNum, @preferredPaymentMethod, @TimeStamp)", con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@userID", user.UserID);
                cmd.Parameters.AddWithValue("@userCode", user.UserCode);
                cmd.Parameters.AddWithValue("@contryRegion", user.CounteryRegion);
                cmd.Parameters.AddWithValue("@isBuyer", user.IsBuyer);
                cmd.Parameters.AddWithValue("@isSeller", user.IsSeller);
                cmd.Parameters.AddWithValue("@isAdmin", 0);
                cmd.Parameters.AddWithValue("@fullName", user.FullName);
                cmd.Parameters.AddWithValue("@phoneNumber", user.PhoneNumber);
                cmd.Parameters.AddWithValue("@email", user.Email);
                cmd.Parameters.AddWithValue("@passwordHash", passwordHash);
                cmd.Parameters.AddWithValue("@passwordSalt", passwordSalt);
                cmd.Parameters.AddWithValue("@address", user.Address);
                cmd.Parameters.AddWithValue("@companyName", user.CompanyName);
                cmd.Parameters.AddWithValue("@website", user.Website);
                cmd.Parameters.AddWithValue("@productCategory", user.ProductCategory);
                cmd.Parameters.AddWithValue("@yearsInBusiness", user.YearsInBusiness);
                cmd.Parameters.AddWithValue("@businessRegNum", user.BusinessRegistrationNumber);
                cmd.Parameters.AddWithValue("@TaxIDNum", user.TaxIDNumber);
                cmd.Parameters.AddWithValue("@preferredPaymentMethod", user.PreferredPaymentMethod);
               cmd.Parameters.AddWithValue("@TimeStamp", DateTime.UtcNow);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
                string encryptedUserCode = CommonServices.EncryptPassword(user.UserCode);
                string role = user.IsAdmin ? "admin" : user.IsSeller ? "seller" : user.IsBuyer ? "buyer" : "";
                string token = CreateToken(role);
                var newRefreshToken = CreateRefreshToken(encryptedUserCode);
            return Ok(new
            {
                message = "User created successfully",
                encryptedUserCode,
                role,
                token,  // Include the token in the response object
                newRefreshToken
            });
            //}
        }

        // =================================================== Login ===================================
        [HttpPost]
        [Route("login")]
        public IActionResult LoginUser(UserModel user)
        //public IActionResult LoginUser(string phone, string pass)
        {
            //UserModel user = new UserModel();
            string encryptedPassword = CommonServices.EncryptPassword(user.Password);
            SqlCommand cmd = new SqlCommand("SELECT * FROM  [UserRegistration] WHERE PhoneNumber = @phoneNumber ", con);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@phoneNumber", user.PhoneNumber);
            cmd.Parameters.AddWithValue("@Password", encryptedPassword);
            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                string encryptedUserCode = CommonServices.EncryptPassword(reader["UserCode"].ToString());
                //user.UserID = (int)reader["user_id"];
                //    user.UserCode = reader["UserCode"].ToString();
                //    user.CounteryRegion = reader["CountryRegion"].ToString();
                user.IsBuyer = (bool)reader["IsBuyer"];
                user.IsSeller = (bool)reader["IsSeller"];
                user.IsAdmin = (bool)reader["IsAdmin"];

                byte[] storedPasswordHash = (byte[])reader["PasswordHash"];
                byte[] storedPasswordSalt = (byte[])reader["PasswordSalt"];
                //    user.FullName = reader["full_name"].ToString();
                //    user.PhoneNumber = reader["PhoneNumber"].ToString();
                //    user.Email = reader["email"].ToString();
                //    user.Address = reader["address"].ToString();
                //    user.CompanyName = reader["company_name"].ToString();
                //    user.Website = reader["website"].ToString();
                //    user.ProductCategory = reader["product_category"].ToString();
                //    user.YearsInBusiness = reader["years_in_business"].ToString();
                //    user.BusinessRegistrationNumber = reader["business_registration_number"].ToString();
                //    user.TaxIDNumber = reader["tax_id_number"].ToString();
                //    user.PreferredPaymentMethod = reader["preferred_payment_method"].ToString();

                con.Close();
                //user.UserCode = encryptedUserCode;
                string role = user.IsAdmin ? "admin" : user.IsSeller ? "seller" : user.IsBuyer ? "buyer" : "";
                string token = CreateToken(role);
                var newRefreshToken = CreateRefreshToken(encryptedUserCode);

                if (!VerifyPasswordHash(user.Password, storedPasswordHash, storedPasswordSalt))
                {
                    return BadRequest(new { message = "Invalid Password" });
                }
                //await SetRefreshToken(newRefreshToken, encryptedUserCode);
                // Return the user object as a response
                return Ok(new { message = "Login successful", encryptedUserCode, role, token, newRefreshToken });
            }
            else
            {
                con.Close();
                return BadRequest(new { message = "Invalid phone number or Password"});
            }
        }
        [HttpPost]
        [Route("GenerateRefreshToken")]
        public IActionResult GenerateRefreshToken([FromForm] string token)
        {
            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken;

            try
            {
                jwtToken = handler.ReadToken(token) as JwtSecurityToken;
                if (jwtToken == null) throw new ArgumentException("Invalid token");
            }
            catch (ArgumentException)
            {
                return Unauthorized("Token is not in a valid JWT format.");
            }

            var issueDate = jwtToken.ValidFrom;
            var expireDate = jwtToken.ValidTo;

            if (DateTime.UtcNow > expireDate)
            {
                // Return a forbidden (403) response
                //  return Unauthorized();
                return Forbid();
                //return Ok(new
                //{
                //    message = "reFreshToken Expired",

                //});
            }

            var userIdClaim = jwtToken.Claims.FirstOrDefault(c =>  c.Type == ClaimTypes.NameIdentifier);
            var encryptedUserId = userIdClaim != null ? userIdClaim.Value : null;
            string userId;
            if (encryptedUserId != null)
            {
                userId = CommonServices.DecryptPassword(encryptedUserId).ToString();
            }
            else
            {
                return BadRequest("The token does not contain the expected claim.");
            }

         

            DateTime? timeStamp = null;
            bool? isBuyer = null;
            bool? isSeller = null;
            bool? isAdmin = null;

            string query = "SELECT * FROM UserRegistration WHERE UserCode = @userId";

            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@userId", userId);

                try
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            timeStamp = reader["TimeStamp"] as DateTime?;
                            isBuyer = reader.GetBoolean(reader.GetOrdinal("IsBuyer"));
                            isSeller = reader.GetBoolean(reader.GetOrdinal("IsSeller"));
                            isAdmin = reader.GetBoolean(reader.GetOrdinal("IsAdmin"));

                        }
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, "Internal server error: " + ex.Message);
                }
            }
            string role = "";
            if ((bool)isBuyer)
            {
                role = "buyer";
            }
            if ((bool)isSeller)
            {
                role = "seller";
            }
            if ((bool)isAdmin)
            {
                role = "admin";
            }


            if (timeStamp == null || timeStamp.Value > issueDate)
            {
                timeStamp = DateTime.UtcNow.AddSeconds(1); // Assign any value greater than issueDate
            }

            // If the timestamp is more recent than the token issue date, it means the token should be considered invalid
            if (timeStamp > issueDate)
            {
                return Unauthorized("Token has been invalidated.");
            }

            // At this point, the token is considered valid, and we can generate a new access and refresh token
            string newAccessToken = CreateToken(role); // Replace "RoleFromYourSystem" with actual role retrieval logic
            string newRefreshToken = CreateRefreshToken(encryptedUserId); // This method should be defined to create a refresh token

            return Ok(new
            {
                
                accessToken = newAccessToken,
                refreshToken = newRefreshToken
            });
        }



        //====================== Token code added by utshow ======================================
        private void createPasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }

        }
        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
        private string CreateToken(string role)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, role)
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);


            var token = new JwtSecurityToken(
                claims: claims,
               expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
        private string CreateRefreshToken(string userId)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
              
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);


            var token = new JwtSecurityToken(
                claims: claims,
                notBefore: DateTime.UtcNow,
 
                expires: DateTime.UtcNow.AddDays(1),
 
                signingCredentials: creds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
       
      


        // ==================================== UPDATE USER ===========================
        [HttpPut]
        [Route("UpdateUser")]

        public IActionResult UpdateUser(UserModel user)
        {

            //string encryptedPassword = EncryptPassword(user.Password);
            //string decryptedUserCode = DecryptPassword(user.UserCode);
            SqlCommand cmd = new SqlCommand("UPDATE UserRegistration SET CountryRegion = @contryRegion, IsBuyer = @isBuyer, IsSeller = @isSeller, FullName = @fullName, PhoneNumber = @phoneNumber, Email = @email, Address = @address, CompanyName = @companyName, Website = @website, ProductCategory = @productCategory, YearsInBusiness = @yearsInBusiness, BusinessRegistrationNumber = @businessRegNum, TaxIdNumber = @TaxIDNum, PreferredPaymentMethod = @preferredPaymentMethod WHERE UserId = @userID", con);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@userID", user.UserID);
            //cmd.Parameters.AddWithValue("@userCode", decryptedUserCode);
            cmd.Parameters.AddWithValue("@contryRegion", user.CounteryRegion);
            cmd.Parameters.AddWithValue("@isBuyer", user.IsBuyer);
            cmd.Parameters.AddWithValue("@isSeller", user.IsSeller);
            //cmd.Parameters.AddWithValue("@isBoth", user.IsBoth);
            cmd.Parameters.AddWithValue("@fullName", user.FullName);
            cmd.Parameters.AddWithValue("@phoneNumber", user.PhoneNumber);
            cmd.Parameters.AddWithValue("@email", user.Email);
            //cmd.Parameters.AddWithValue("@Password", encryptedPassword);
            cmd.Parameters.AddWithValue("@address", user.Address);
            cmd.Parameters.AddWithValue("@companyName", user.CompanyName);
            cmd.Parameters.AddWithValue("@website", user.Website);
            cmd.Parameters.AddWithValue("@productCategory", user.ProductCategory);
            cmd.Parameters.AddWithValue("@yearsInBusiness", user.YearsInBusiness);
            cmd.Parameters.AddWithValue("@businessRegNum", user.BusinessRegistrationNumber);
            cmd.Parameters.AddWithValue("@TaxIDNum", user.TaxIDNumber);
            cmd.Parameters.AddWithValue("@preferredPaymentMethod", user.PreferredPaymentMethod);
            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();
            string encryptedUserCode = CommonServices.EncryptPassword(user.UserCode);
            return Ok(new { message = "User updated successfully", user });
                  
        }

       

        // =================================================== getSingleUserInfo ===================================
        [HttpGet]
        [Route("getSingleUserInfo")]
        public IActionResult getSingleUser(string  userCode)
        {
            UserModel user = new UserModel();
            //byte[] userCodeBytes = Encoding.UTF8.GetBytes(userCode);
            string decryptedUserCode = CommonServices.DecryptPassword(userCode);
            //string DecryptedUserCode = ConvertBytesToHexString(user.UserCode);
            SqlCommand cmd = new SqlCommand("SELECT * FROM UserRegistration WHERE UserCode = @userCode ", con);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@userCode", decryptedUserCode);
            //Console.WriteLine(decryptedUserCode);
            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                user.UserID = (int)reader["UserId"];
                user.UserCode = reader["UserCode"].ToString();
                user.CounteryRegion = reader["CountryRegion"].ToString();
                user.IsBuyer = (bool)reader["IsBuyer"];
                user.IsSeller = (bool)reader["IsSeller"];
                user.IsAdmin = (bool)reader["IsAdmin"];
                user.FullName = reader["FullName"].ToString();
                user.PhoneNumber = reader["PhoneNumber"].ToString();
                user.Email = reader["Email"].ToString();
                user.Address = reader["Address"].ToString();
                user.CompanyName = reader["CompanyName"].ToString();
                user.Website = reader["Website"].ToString();
                user.ProductCategory = reader["ProductCategory"].ToString();
                user.YearsInBusiness = reader["YearsInBusiness"].ToString();
                user.BusinessRegistrationNumber = reader["BusinessRegistrationNumber"].ToString();
                user.TaxIDNumber = reader["TaxIdNumber"].ToString();
                user.PreferredPaymentMethod = reader["PreferredPaymentMethod"].ToString();

                con.Close();

                // Return the user object as a response
                return Ok(new { message = "GET single data successful", user });
            }
            else
            {
                con.Close();
                return BadRequest(new { message = "Invalid Inforamtion" });
            }
        }



        //// =================================================== isAdmin ===================================
        //[HttpGet]
        //[Route("isAdmin")]
        //public IActionResult isAdmin(string userCode)
        //{
        //    UserModel user = new UserModel();
     
        //    string decryptedUserCode = CommonServices.DecryptPassword(userCode);
        
        //    SqlCommand cmd = new SqlCommand("SELECT PhoneNumber FROM UserRegistration WHERE UserCode = @userCode ", con);
        //    cmd.CommandType = CommandType.Text;
        //    cmd.Parameters.AddWithValue("@userCode", decryptedUserCode);
        //    //Console.WriteLine(decryptedUserCode);
        //    con.Open();
        //    SqlDataReader reader = cmd.ExecuteReader();
        //    if (reader.Read())
        //    {
        //        con.Close();

        //        return Ok(new { message = "User is an Admin" });
        //    }
        //    else
        //    {
        //        con.Close();
        //        return BadRequest(new { message = "User is not an Admin" });
        //    }
        //}


        // ============================= Update Pass =============================
        [HttpPut]
        [Route("updatePass")]
        public IActionResult UpdatePasss(UpdatePasswordModel user)
        {
            SqlCommand cmd = new SqlCommand("SELECT * FROM UserRegistration WHERE UserCode = @userCode ", con);
           
            string decrypteedUserCode = CommonServices.DecryptPassword(user.userCode);
            cmd.Parameters.AddWithValue("@userCode", decrypteedUserCode);
         
         
            createPasswordHash(user.newPassword, out byte[] passwordHash, out byte[] passwordSalt);

            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                byte[] storedPasswordHash = (byte[])reader["PasswordHash"];
                byte[] storedPasswordSalt = (byte[])reader["PasswordSalt"];


                reader.Close();
                if (!VerifyPasswordHash(user.oldPassword, storedPasswordHash, storedPasswordSalt))
                {
                    return BadRequest(new { message = "Password did not matched!" });
                }

                SqlCommand cmd2 = new SqlCommand("UPDATE UserRegistration SET [PasswordHash] = @passwordHash ,[PasswordSalt] =@passwordSalt WHERE UserCode = @userCode ", con);
                cmd2.Parameters.AddWithValue("@userCode", decrypteedUserCode);
                cmd2.Parameters.AddWithValue("@passwordHash", passwordHash);
                cmd2.Parameters.AddWithValue("@passwordSalt", passwordSalt);
                //cmd2.Parameters.AddWithValue("@OldPasswordHash", oldpasswordHash);
                int rowsAffected = cmd2.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    con.Close();
                    return Ok(new { message = "Passsword updated successfully!", user.userCode });
                }
            }

            con.Close();
            return BadRequest(new { message = "Password did not matched!" });
        }

    }
}
