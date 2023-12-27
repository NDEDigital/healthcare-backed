using Microsoft.AspNetCore.Mvc;
using NDE_Digital_Market.Model.MaterialStock;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace NDE_Digital_Market.SharedServices
{
    public class CommonServices
    {
        //public readonly string FilesPath = @"F:\Projects\Health Care\healthcare-frontend\src\assets\images\";
        //public static string FilesPath { get; } = @"F:\Projects\Health Care\healthcare-frontend\src\assets\images\";

        public string FilesPath { get; set; }
        public string HealthCareConnection { get; set; }
        private readonly IConfiguration _configuration;
        private readonly SqlConnection con;
       
        public CommonServices(IConfiguration configuration)
        {
            _configuration = configuration;
            FilesPath = _configuration["FilesPaths:filepath"];
            HealthCareConnection = configuration.GetConnectionString("HealthCare");
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
      

        public static string UploadFiles(string foldername, string filename, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return "Invalid file";
            }

            // Get the original file name with extension
            string fileNameWithExtension = file.FileName;

            // Generate a unique file name
            var uniqueFileName = GetUniqueFileName(filename, fileNameWithExtension);

            // Combine the unique file name with the upload path
            var filePath = Path.Combine(foldername, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(fileStream);
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
        private static string GetUniqueFileName(string fileName, string fileNameWithExtension)
        {
            //string fileNameWE = Path.GetFileNameWithoutExtension(fileName, );
            return $"{fileName}_{DateTime.Now.Ticks}{Path.GetExtension(fileNameWithExtension)}";
        }



    }
}
