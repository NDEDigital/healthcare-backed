using Microsoft.AspNetCore.Mvc;
using NDE_Digital_Market.Model.MaterialStock;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace NDE_Digital_Market.SharedServices
{
    public class CommonServices
    {
        
        private readonly IConfiguration _configuration;
        private readonly SqlConnection con;
       
        public CommonServices(IConfiguration configuration)
        {
            _configuration = configuration;
          
            con = new SqlConnection(_configuration.GetConnectionString("DigitalMarketConnection"));
           
        }

        public static string EncryptPassword(string clearText)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        ////DecryptPassword
        public static string DecryptPassword(string cipherText)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
        public string InsertUpdateStockQt(List<MaterialStock> Stock)
        {

            string query = @"IF EXISTS (
                            SELECT 1 
                            FROM MaterialStockQty
                            WHERE GroupCode = @GroupCode AND GoodsId = @GoodsId AND SellerCode = @SellerCode
                        )
                        BEGIN
                            IF @OperationType = 'ADD'
                            BEGIN
                                UPDATE MaterialStockQty
                                SET PreviousQty = PresentQty, PresentQty = PresentQty + @SALES
                                WHERE GroupCode = @GroupCode AND GoodsId = @GoodsId AND SellerCode = @SellerCode
                            END
                            ELSE IF @OperationType = 'SUBTRACT'
                            BEGIN
                                UPDATE MaterialStockQty
                                SET PreviousQty = PresentQty, PresentQty = PresentQty - @SALES
                                WHERE GroupCode = @GroupCode AND GoodsId = @GoodsId AND SellerCode = @SellerCode
                            END
                        END
                        ELSE
                        BEGIN
                            INSERT INTO MaterialStockQty (GroupCode, GoodsId, SellerCode, PreviousQty, PresentQty)
                            VALUES (@GroupCode, @GoodsId, @SellerCode, @PreviousQty, @PresentQty)           
                        END";

            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                foreach (var stock in Stock)
                {
                    cmd.Parameters.AddWithValue("@GroupCode", stock.GroupCode);
                    cmd.Parameters.AddWithValue("@GoodsId", stock.GoodsId);
                    cmd.Parameters.AddWithValue("@SellerCode", stock.SellerCode);
                    cmd.Parameters.AddWithValue("@SALES", stock.SalesQty);
                    cmd.Parameters.AddWithValue("@OperationType", stock.OperationType);
                    cmd.Parameters.AddWithValue("@PreviousQty", 10);
                    cmd.Parameters.AddWithValue("@PresentQty", 20);

                    try
                    {
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                        return "200"; // If execution is successful
                    }
                    catch (SqlException ex)
                    {
                        // Handle any SQL-related errors
                        return "SQL Error";
                    }
                    catch (Exception ex)
                    {
                        // Handle any other errors
                        return "500";
                    }

                }
            }
            return "200";


        }

    }
}
