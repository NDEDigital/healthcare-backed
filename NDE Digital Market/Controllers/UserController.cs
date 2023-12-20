using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NDE_Digital_Market.Model;
using NDE_Digital_Market.DTOs;
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
        private readonly SqlConnection _healthCareConnection;
        public UserController(IConfiguration configuration)
        {
            _configuration = configuration;
            con = new SqlConnection(_configuration.GetConnectionString("ProminentConnection"));
            _healthCareConnection = new SqlConnection(_configuration.GetConnectionString("HealthCare"));
           
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
        
        private async Task<bool> CompanyExistAsync(string CompanyCode)
        {
            string query = @"SELECT CASE WHEN COUNT(*) < cr.MaxUser THEN 0 ELSE 1 END AS UserCount
                                FROM UserRegistration UR
                                JOIN CompanyRegistration CR ON UR.CompanyCode = CR.CompanyCode
	                                WHERE UR.CompanyCode=CR.CompanyCode
	                                AND UR.IsActive=1
                                And CR.CompanyCode=@CompanyCode
                                GROUP BY CR.MaxUser";

            try
            {
                await _healthCareConnection.OpenAsync();

                using (var cmd = new SqlCommand(query, _healthCareConnection))
                    {
                                cmd.CommandType = CommandType.Text;
                                cmd.Parameters.AddWithValue("@CompanyCode", CompanyCode);

                                // Execute the query and store the result in the 'userCount' variable
                                var result = await cmd.ExecuteScalarAsync();
                                await _healthCareConnection.CloseAsync();

                                // Check if result is not null and cast to int
                                int userCount = result != null ? Convert.ToInt32(result) : 0;   
                                if(userCount == 0)
                                {
                                    return false; 
                                }
                    }
                    
               }
            
            catch (Exception ex)
            {
                 return false;
            }

            return true;
        }

    
        [HttpPost]
        [Route("CreateUser")]
        public async Task<IActionResult> CreateUser(CreateUserDto user)
        {

            if (user.CompanyId != null)
            {
                if (await CompanyExistAsync(user.CompanyId) == false)
                {
                    return BadRequest("Conpany Code problem");
                }

            }

            string systemCode = string.Empty;

                // Execute the stored procedure to generate the system code
                SqlCommand cmdSP = new SqlCommand("spMakeSystemCode", _healthCareConnection);
                {
                    cmdSP.CommandType = CommandType.StoredProcedure;
                    cmdSP.Parameters.AddWithValue("@TableName", "UserRegistration");
                    cmdSP.Parameters.AddWithValue("@Date", DateTime.Now.ToString("yyyy-MM-dd"));
                    cmdSP.Parameters.AddWithValue("@AddNumber", 1);

     
                   await _healthCareConnection.OpenAsync();
                    systemCode = cmdSP.ExecuteScalar()?.ToString();
                   await _healthCareConnection.CloseAsync();
                }

            
                //SP END

                // Encrypt the Password
            string encryptedPassword = CommonServices.EncryptPassword(user.Password);
            createPasswordHash(user.Password, out byte[] passwordHash, out byte[] passwordSalt);

            UserModel userModel = new UserModel();
            userModel.UserId = int.Parse(systemCode.Split('%')[0]);
            userModel.UserCode = systemCode.Split('%')[1];
            userModel.IsBuyer = user.IsBuyer;
            userModel.IsSeller = user.IsSeller;
            userModel.IsAdmin = user.IsAdmin;
            userModel.FullName = user.FullName;
            userModel.PhoneNumber = user.PhoneNumber;
            userModel.Email = user.Email;
            userModel.PasswordHash = passwordHash;
            userModel.PasswordSalt = passwordSalt;
            userModel.Address = user.Address;
            userModel.AddedDate = DateTime.UtcNow;
            userModel.CompanyId = user.CompanyId;

            string query = @"
                            INSERT INTO UserRegistration (
                                UserId, UserCode, IsBuyer, IsSeller, IsAdmin, 
                                FullName, PhoneNumber, Email, PasswordHash, PasswordSalt, Address,                           
                                TimeStamp,IsActive, AddedDate,CompanyCode
                            ) VALUES (
                                @UserID, @UserCode,  @IsBuyer, @IsSeller, @IsAdmin, 
                                @FullName, @PhoneNumber, @Email, @PasswordHash, @PasswordSalt, @Address, 
                                @TimeStamp,@IsActive,@AddedDate,@CompanyCode
                            )";


            SqlCommand cmd = new SqlCommand(query, _healthCareConnection);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@UserID", userModel.UserId);
            cmd.Parameters.AddWithValue("@UserCode", userModel.UserCode ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@IsBuyer", userModel.IsBuyer.HasValue ? (object)userModel.IsBuyer.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@IsSeller", userModel.IsSeller.HasValue ? (object)userModel.IsSeller.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@IsAdmin", userModel.IsAdmin.HasValue ? (object)userModel.IsAdmin.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@FullName", userModel.FullName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@PhoneNumber", userModel.PhoneNumber ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Email", userModel.Email ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@PasswordHash", userModel.PasswordHash ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@PasswordSalt", userModel.PasswordSalt ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Address", userModel.Address ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@AddedDate", userModel.AddedDate.HasValue ? (object)userModel.AddedDate.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@TimeStamp", userModel.AddedDate.HasValue ? (object)userModel.AddedDate.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@CompanyCode", userModel.CompanyId);
            cmd.Parameters.AddWithValue("@IsActive",true);

            await _healthCareConnection.OpenAsync();
               await  cmd.ExecuteNonQueryAsync();
               await _healthCareConnection.CloseAsync();
                string encryptedUserCode = CommonServices.EncryptPassword(userModel.UserId.ToString());
                string role = userModel.IsAdmin == true ? "admin" :
                              userModel.IsSeller == true ? "seller" :
                              userModel.IsBuyer == true ? "buyer" :
                              "";
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
        public async Task<IActionResult> LoginUser(LoginUserDto user)
        //public IActionResult LoginUser(string phone, string pass)
        {
            //UserModel user = new UserModel();
            string encryptedPassword = CommonServices.EncryptPassword(user.Password);
            SqlCommand cmd = new SqlCommand("SELECT * FROM  [UserRegistration] WHERE PhoneNumber = @phoneNumber ", _healthCareConnection);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@phoneNumber", user.PhoneNumber);
            cmd.Parameters.AddWithValue("@Password", encryptedPassword);
           await _healthCareConnection.OpenAsync();
            SqlDataReader reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                string encryptedUserCode = CommonServices.EncryptPassword(reader["UserCode"].ToString());
     
                bool IsBuyer = (bool)reader["IsBuyer"];
                bool IsSeller = (bool)reader["IsSeller"];
                bool IsAdmin = (bool)reader["IsAdmin"];

                byte[] storedPasswordHash = (byte[])reader["PasswordHash"];
                byte[] storedPasswordSalt = (byte[])reader["PasswordSalt"];
                await _healthCareConnection.CloseAsync();
              
                string role = IsAdmin ? "admin" : IsSeller ? "seller" : IsBuyer ? "buyer" : "";
                string token = CreateToken(role);
                var newRefreshToken = CreateRefreshToken(encryptedUserCode);

                if (!VerifyPasswordHash(user.Password, storedPasswordHash, storedPasswordSalt))
                {
                    return BadRequest(new { message = "Invalid password" });
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
        public async Task<IActionResult> GenerateRefreshToken([FromForm] string token)
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

            string query = "SELECT * FROM UserRegistration WHERE UserId = @userId";

            using (SqlCommand cmd = new SqlCommand(query, _healthCareConnection))
            {
                cmd.Parameters.AddWithValue("@userId", userId);

                try
                {
                  await _healthCareConnection.OpenAsync();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
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
        //[HttpPut]
        //[Route("UpdateUser")]

        //public IActionResult UpdateUser(UserModel user)
        //{

        //    //string encryptedPassword = EncryptPassword(user.Password);
        //    //string decryptedUserCode = DecryptPassword(user.UserCode);
        //    SqlCommand cmd = new SqlCommand("UPDATE UserRegistration SET CountryRegion = @contryRegion, IsBuyer = @isBuyer, IsSeller = @isSeller, FullName = @fullName, PhoneNumber = @phoneNumber, Email = @email, Address = @address, CompanyName = @companyName, Website = @website, ProductCategory = @productCategory, YearsInBusiness = @yearsInBusiness, BusinessRegistrationNumber = @businessRegNum, TaxIdNumber = @TaxIDNum, PreferredPaymentMethod = @preferredPaymentMethod WHERE UserId = @userID", con);
        //    cmd.CommandType = CommandType.Text;
        //    cmd.Parameters.AddWithValue("@userID", user.UserID);
        //    //cmd.Parameters.AddWithValue("@userCode", decryptedUserCode);
        //    cmd.Parameters.AddWithValue("@contryRegion", user.CounteryRegion);
        //    cmd.Parameters.AddWithValue("@isBuyer", user.IsBuyer);
        //    cmd.Parameters.AddWithValue("@isSeller", user.IsSeller);
        //    //cmd.Parameters.AddWithValue("@isBoth", user.IsBoth);
        //    cmd.Parameters.AddWithValue("@fullName", user.FullName);
        //    cmd.Parameters.AddWithValue("@phoneNumber", user.PhoneNumber);
        //    cmd.Parameters.AddWithValue("@email", user.Email);
        //    //cmd.Parameters.AddWithValue("@Password", encryptedPassword);
        //    cmd.Parameters.AddWithValue("@address", user.Address);
        //    cmd.Parameters.AddWithValue("@companyName", user.CompanyName);
        //    cmd.Parameters.AddWithValue("@website", user.Website);
        //    cmd.Parameters.AddWithValue("@productCategory", user.ProductCategory);
        //    cmd.Parameters.AddWithValue("@yearsInBusiness", user.YearsInBusiness);
        //    cmd.Parameters.AddWithValue("@businessRegNum", user.BusinessRegistrationNumber);
        //    cmd.Parameters.AddWithValue("@TaxIDNum", user.TaxIDNumber);
        //    cmd.Parameters.AddWithValue("@preferredPaymentMethod", user.PreferredPaymentMethod);
        //    con.Open();
        //    cmd.ExecuteNonQuery();
        //    con.Close();
        //    string encryptedUserCode = CommonServices.EncryptPassword(user.UserCode);
        //    return Ok(new { message = "User updated successfully", user });
                  
        //}

       

        // =================================================== getSingleUserInfo ===================================
        //[HttpGet]
        //[Route("getSingleUserInfo")]
        //public IActionResult getSingleUser(string  userCode)
        //{
        //    UserModel user = new UserModel();
        //    //byte[] userCodeBytes = Encoding.UTF8.GetBytes(userCode);
        //    string decryptedUserCode = CommonServices.DecryptPassword(userCode);
        //    //string DecryptedUserCode = ConvertBytesToHexString(user.UserCode);
        //    SqlCommand cmd = new SqlCommand("SELECT * FROM UserRegistration WHERE UserCode = @userCode ", con);
        //    cmd.CommandType = CommandType.Text;
        //    cmd.Parameters.AddWithValue("@userCode", decryptedUserCode);
        //    //Console.WriteLine(decryptedUserCode);
        //    con.Open();
        //    SqlDataReader reader = cmd.ExecuteReader();
        //    if (reader.Read())
        //    {
        //        user.UserID = (int)reader["UserId"];
        //        user.UserCode = reader["UserCode"].ToString();
        //        user.CounteryRegion = reader["CountryRegion"].ToString();
        //        user.IsBuyer = (bool)reader["IsBuyer"];
        //        user.IsSeller = (bool)reader["IsSeller"];
        //        user.IsAdmin = (bool)reader["IsAdmin"];
        //        user.FullName = reader["FullName"].ToString();
        //        user.PhoneNumber = reader["PhoneNumber"].ToString();
        //        user.Email = reader["Email"].ToString();
        //        user.Address = reader["Address"].ToString();
        //        user.CompanyName = reader["CompanyName"].ToString();
        //        user.Website = reader["Website"].ToString();
        //        user.ProductCategory = reader["ProductCategory"].ToString();
        //        user.YearsInBusiness = reader["YearsInBusiness"].ToString();
        //        user.BusinessRegistrationNumber = reader["BusinessRegistrationNumber"].ToString();
        //        user.TaxIDNumber = reader["TaxIdNumber"].ToString();
        //        user.PreferredPaymentMethod = reader["PreferredPaymentMethod"].ToString();

        //        con.Close();

        //        // Return the user object as a response
        //        return Ok(new { message = "GET single data successful", user });
        //    }
        //    else
        //    {
        //        con.Close();
        //        return BadRequest(new { message = "Invalid Inforamtion" });
        //    }
        //}



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
