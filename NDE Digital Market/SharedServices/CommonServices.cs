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
        public string InsertStockQt(MaterialStockInsert Stock)
        {

            string query = @"
                            INSERT INTO MaterialStockQty (GroupCode, GoodsId, SellerCode, PreviousQty, PresentQty)
                            VALUES (@GroupCode, @GoodsId, @SellerCode, @PreviousQty, @PresentQty)           
                       ";

            using (SqlCommand cmd = new SqlCommand(query, con))
            {
               
                    cmd.Parameters.AddWithValue("@GroupCode", Stock.GroupCode);
                    cmd.Parameters.AddWithValue("@GoodsId", Stock.GoodsId);
                    cmd.Parameters.AddWithValue("@SellerCode", Stock.SellerCode);
                    cmd.Parameters.AddWithValue("@PreviousQty", Stock.PreviousQty);
                    cmd.Parameters.AddWithValue("@PresentQty", Stock.PresentQty);

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
            return "200";


        }
        public string UpdateStockQt(List<MaterialStockUpdate> Stock)
        {

            string query = @"UPDATE MaterialStockQty SET PreviousQty = PresentQty, PresentQty = PresentQty - @SALES
                              WHERE GroupCode = @GroupCode AND GoodsId = @GoodsId AND SellerCode = @SellerCode";

            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                foreach (var stock in Stock)
                {
                    cmd.Parameters.AddWithValue("@GroupCode", stock.GroupCode);
                    cmd.Parameters.AddWithValue("@GoodsId", stock.GoodsId);
                    cmd.Parameters.AddWithValue("@SellerCode", stock.SellerCode);
                    cmd.Parameters.AddWithValue("@SALES", stock.SalesQty);
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


        public static string UploadFiles(string foldername, string filename, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return "Invalid file";
            }


            // Generate a unique file name
            var uniqueFileName = GetUniqueFileName(filename);

            // Combine the unique file name with the upload path
            var filePath = Path.Combine(foldername, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyToAsync(fileStream);
            }
            return filePath;

        }

        public static byte[] GetFiles(string filepath)
        {
            if (!System.IO.File.Exists(filepath))
            {
                return null;
            }

            return System.IO.File.ReadAllBytes(filepath);
        }


        // Generate a unique file name to avoid overwriting existing files
        private static string GetUniqueFileName(string fileName)
        {
            string fileNameWE = Path.GetFileNameWithoutExtension(fileName);
            return $"{fileNameWE}_{DateTime.Now.Ticks}{Path.GetExtension(fileName)}";
        }



    }
}
