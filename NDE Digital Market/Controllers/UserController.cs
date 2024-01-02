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
using NDE_Digital_Market.SharedServices;

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
            CommonServices commonServices = new CommonServices(configuration);
            con = new SqlConnection(_configuration.GetConnectionString("ProminentConnection"));
            _healthCareConnection = new SqlConnection(commonServices.HealthCareConnection);
           
        }

        //===================================== Create User ================================
        [HttpPost]
        [Route("UserExist")]
        public async Task<bool> UserExist(CreateUserDto user)
        {
            SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM  [UserRegistration] WHERE PhoneNumber = @phoneNumber", _healthCareConnection);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@phoneNumber", user.PhoneNumber);
            await _healthCareConnection.OpenAsync();
            int count = (int)await cmd.ExecuteScalarAsync();
            await _healthCareConnection.CloseAsync();
            Boolean userExist = false;
            if (count > 0)
            {
                userExist = true;
            }
            return userExist;
        }

        private async Task<int?> CompanyExistAsync(string CompanyCode)
        {
            string query = @"SELECT COALESCE(CASE WHEN COUNT(UR.CompanyCode) < CR.MaxUser THEN 1 ELSE 0 END, 0) AS UserCount
                                FROM CompanyRegistration CR
                                LEFT JOIN UserRegistration UR ON UR.CompanyCode = CR.CompanyCode AND UR.IsActive = 1
                                WHERE CR.CompanyCode = @CompanyCode
								and CR.IsActive = 1
                                GROUP BY CR.MaxUser;";

            try
            {
                await _healthCareConnection.OpenAsync();

                using (var cmd = new SqlCommand(query, _healthCareConnection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@CompanyCode", CompanyCode);
                    var result = await cmd.ExecuteScalarAsync();

                    await _healthCareConnection.CloseAsync();
                    return result != null ? Convert.ToInt32(result) : (int?)null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        [HttpPost]
        [Route("CreateUser")]
        public async Task<IActionResult> CreateUser(CreateUserDto user)
        {
            int? companyExist = 0;

            if (!string.IsNullOrEmpty(user.CompanyCode))
            {
                companyExist = await CompanyExistAsync(user.CompanyCode);
                if (companyExist == null)
                {
                    return BadRequest(new
                    {
                        message = "No Campany Found with this ID"
                    });
                }
                else if(companyExist == 0)
                {
                    return BadRequest(new
                    {
                        message = "Max user count exited for this company!"
                    });
                }

            }
            var userExistResult = await UserExist(user);
            if (userExistResult)
            {
                return BadRequest(new { message = "User already exists" });
            }
            string systemCode = string.Empty;

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

            if (companyExist == 1)
            {
                string updateCompanyAdmin = @"
                      UPDATE CompanyRegistration
                        SET CompanyAdminId = @CompanyAdminId
                        WHERE CompanyAdminId IS NULL AND CompanyCode = @CompanyCode;
                        ";
                    SqlCommand cmd1 = new SqlCommand(updateCompanyAdmin, _healthCareConnection);
                    cmd1.CommandType = CommandType.Text;
                    cmd1.Parameters.AddWithValue("@CompanyCode", user.CompanyCode);
                    cmd1.Parameters.AddWithValue("@CompanyAdminId", int.Parse(systemCode.Split('%')[0]));

                    await _healthCareConnection.OpenAsync();
                    int rowsAffected = cmd1.ExecuteNonQuery();
                    await _healthCareConnection.CloseAsync();
            }

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
            userModel.CompanyCode = user.CompanyCode ?? string.Empty;

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
            cmd.Parameters.AddWithValue("@CompanyCode", userModel.CompanyCode);
            cmd.Parameters.AddWithValue("@IsActive",true);

            await _healthCareConnection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
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
                token, 
                newRefreshToken
            });

        }


        // =================================================== Login ===================================
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> LoginUser(LoginUserDto user)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM  [UserRegistration] WHERE PhoneNumber = @phoneNumber ", _healthCareConnection);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@phoneNumber", user.PhoneNumber);

                await _healthCareConnection.OpenAsync();
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    int userId = (int)reader["UserId"];
                    bool IsBuyer = (bool)reader["IsBuyer"];
                    bool IsSeller = (bool)reader["IsSeller"];
                    bool IsAdmin = (bool)reader["IsAdmin"];

                    byte[] storedPasswordHash = (byte[])reader["PasswordHash"];
                    byte[] storedPasswordSalt = (byte[])reader["PasswordSalt"];

                    await _healthCareConnection.CloseAsync();

                    string role = IsAdmin ? "admin" : IsSeller ? "seller" : IsBuyer ? "buyer" : "";
                    string token = CreateToken(role);
                    var newRefreshToken = CreateRefreshToken(userId.ToString());

                    if (!VerifyPasswordHash(user.Password, storedPasswordHash, storedPasswordSalt))
                    {
                        return BadRequest(new { message = "Invalid password" });
                    }

                    return Ok(new { message = "Login successful", userId, role, token, newRefreshToken });
                }
                else
                {
                    return BadRequest(new { message = "Invalid phone number or Password" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return BadRequest(new { message = "An error occurred while processing the request." });
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
                return Forbid();
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
                timeStamp = DateTime.UtcNow.AddSeconds(1); 
            }

            if (timeStamp > issueDate)
            {
                return Unauthorized("Token has been invalidated.");
            }


            string newAccessToken = CreateToken(role); 
            string newRefreshToken = CreateRefreshToken(encryptedUserId); 

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
        [HttpGet]
        [Route("getSingleUserInfo")]
        public IActionResult getSingleUser(int? userId)
        {
            try
            {
                UserModel user = new UserModel();
                SqlCommand cmd = new SqlCommand("SELECT * FROM UserRegistration WHERE UserId = @UserId ", _healthCareConnection);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@UserId", userId);

                _healthCareConnection.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    user.UserId = (int)reader["UserId"];
                    user.UserCode = reader["UserCode"].ToString();
                    user.FullName = reader["FullName"].ToString();
                    user.PhoneNumber = reader["PhoneNumber"].ToString();
                    user.Email = reader["Email"].ToString();
                    user.Address = reader["Address"].ToString();
                    _healthCareConnection.Close();

                    return Ok(new { message = "GET single data successful", user });
                }
                else
                {
                    _healthCareConnection.Close();
                    return BadRequest(new { message = "Invalid Information" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return BadRequest(new { message = "An error occurred while processing the request." });
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
            SqlCommand cmd = new SqlCommand("SELECT * FROM UserRegistration WHERE UserId = @UserId", _healthCareConnection);

            cmd.Parameters.AddWithValue("@UserId", user.userId);

            createPasswordHash(user.newPassword, out byte[] passwordHash, out byte[] passwordSalt);

            _healthCareConnection.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();
                byte[] storedPasswordHash = (byte[])reader["PasswordHash"];
                byte[] storedPasswordSalt = (byte[])reader["PasswordSalt"];
                reader.Close();

                if (!VerifyPasswordHash(user.oldPassword, storedPasswordHash, storedPasswordSalt))
                {
                    return BadRequest(new { message = "Password did not match!" });
                }

                SqlCommand cmd2 = new SqlCommand("UPDATE UserRegistration SET [PasswordHash] = @passwordHash, [PasswordSalt] = @passwordSalt WHERE UserId = @userId", _healthCareConnection);
                cmd2.Parameters.AddWithValue("@userId", user.userId);
                cmd2.Parameters.AddWithValue("@passwordHash", passwordHash);
                cmd2.Parameters.AddWithValue("@passwordSalt", passwordSalt);

                int rowsAffected = cmd2.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    _healthCareConnection.Close();
                    return Ok(new { message = "Password updated successfully!", user.userCode });
                }
            }

            _healthCareConnection.Close();
            return BadRequest(new { message = "Password did not match!" });
        }

    }
}
