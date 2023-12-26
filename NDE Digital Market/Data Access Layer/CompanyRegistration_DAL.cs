using NDE_Digital_Market.Data_Access_Layer;
using NDE_Digital_Market.Model;
using NDE_Digital_Market.DTOs;
using System.Data;
using System.Data.SqlClient;
using NDE_Digital_Market.SharedServices;
using NDE_Digital_Market.Controllers;

namespace NDE_Digital_Market.Data_Access_Layer;

public class CompanyRegistration_DAL
{
    //CommonServices commonServices = new CommonServices(_configuration);
    //string foldername = commonServices.FilesPath + "CompanyFiles";

    private readonly IConfiguration _configuration;
    private readonly SqlConnection connection;
    private readonly string foldername;
    private readonly string filename = "companyfiles";
    public CompanyRegistration_DAL(IConfiguration configuration)
    {
        _configuration = configuration;
        connection = new SqlConnection(_configuration.GetConnectionString("HealthCare"));
        CommonServices commonServices = new CommonServices(_configuration);
        foldername = commonServices.FilesPath + "CompanyFiles";
    }
    public async Task<Boolean> CompanyExistAsync(CompanyDto companyDto)
    {
        SqlCommand cmd = new SqlCommand("CheckCompanyExistence", connection);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@CompanyName", companyDto.CompanyName);
        cmd.Parameters.AddWithValue("@BusinessRegistrationNumber", companyDto.BusinessRegistrationNumber);
        cmd.Parameters.AddWithValue("@TaxIdentificationNumber", companyDto.TaxIdentificationNumber);
        await connection.OpenAsync();
        int count = (int)await cmd.ExecuteScalarAsync();
        await connection.CloseAsync();
        Boolean companyNameExist = false;
        if (count > 0)
        {
            companyNameExist = true;
        }
        return companyNameExist;
        //   return BadRequest(new { message = "User does not exist" , userExist });
    }

    public async Task<string> CompanyRegistrationPostAsync(CompanyDto companyDto)
    {
        Boolean companyNameExist = await CompanyExistAsync(companyDto);

        if (companyNameExist)
        {
            return null;
        }
        else
        {
            string systemCode = string.Empty;

            // Execute the stored procedure to generate the system code
            SqlCommand cmdSP = new SqlCommand("spMakeSystemCode", connection);
            {
                cmdSP.CommandType = CommandType.StoredProcedure;
                cmdSP.Parameters.AddWithValue("@TableName", "CompanyRegistration");
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
                await connection.OpenAsync();
                var tempSystem = await cmdSP.ExecuteScalarAsync();
                systemCode = tempSystem?.ToString() ?? string.Empty;
                await connection.CloseAsync();
            }

            string CompanyImage = CommonServices.UploadFiles(foldername, filename, companyDto.CompanyImageFile);
            string TradeLicense = CommonServices.UploadFiles(foldername, filename, companyDto.TradeLicenseFile);
            int CompanyID = int.Parse(systemCode.Split('%')[0]);
            string CompanyCode = systemCode.Split('%')[1];
            //SP END

            SqlCommand cmd = new SqlCommand("INSERT INTO CompanyRegistration (CompanyID, CompanyCode, CompanyName,Email, CompanyImage, " +
                           "CompanyFoundationDate, BusinessRegistrationNumber, TaxIdentificationNumber, " +
                           "TradeLicense, PreferredPaymentMethodID, BankNameID, AccountNumber, " +
                           "AccountHolderName,MaxUser,IsActive, AddedBy, DateAdded, AddedPC) " +
                           "VALUES (@CompanyID, @CompanyCode, @CompanyName,@Email, @CompanyImage, " +
                           "@CompanyFoundationDate, @BusinessRegistrationNumber, @TaxIdentificationNumber, " +
                           "@TradeLicense, @PreferredPaymentMethodID, @BankNameID, @AccountNumber, " +
                           "@AccountHolderName, @MaxUser, @IsActive, @AddedBy, @DateAdded, @AddedPC);", connection);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@CompanyID", CompanyID);
            cmd.Parameters.AddWithValue("@CompanyCode", CompanyCode);
            cmd.Parameters.AddWithValue("@CompanyName", companyDto.CompanyName);
            cmd.Parameters.AddWithValue("@Email", companyDto.Email);
            cmd.Parameters.AddWithValue("@CompanyImage", CompanyImage);
            cmd.Parameters.AddWithValue("@CompanyFoundationDate", companyDto.CompanyFoundationDate);
            cmd.Parameters.AddWithValue("@BusinessRegistrationNumber", companyDto.BusinessRegistrationNumber);
            cmd.Parameters.AddWithValue("@TaxIdentificationNumber", companyDto.TaxIdentificationNumber);
            cmd.Parameters.AddWithValue("@TradeLicense", TradeLicense);
            cmd.Parameters.AddWithValue("@PreferredPaymentMethodID", companyDto.PreferredPaymentMethodID);
            cmd.Parameters.AddWithValue("@BankNameID", companyDto.BankNameID ?? 0);
            cmd.Parameters.AddWithValue("@AccountNumber", companyDto.AccountNumber ?? string.Empty);
            cmd.Parameters.AddWithValue("@AccountHolderName", companyDto.AccountHolderName ?? string.Empty);
            cmd.Parameters.AddWithValue("@MaxUser", 3);
            cmd.Parameters.AddWithValue("@IsActive", -1);
            cmd.Parameters.AddWithValue("@AddedBy", companyDto.AddedBy);
            cmd.Parameters.AddWithValue("@DateAdded", DateTime.Now);
            cmd.Parameters.AddWithValue("@AddedPC", companyDto.AddedPC);

            await connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await connection.CloseAsync();

            return "Company Registration successfull!.";
        }



    }

    public async Task<List<CompanyModel>> GetCompaniesAsync(int status)
    {
        List<CompanyModel> companies = new List<CompanyModel>();
        SqlCommand command = new SqlCommand("GetCompaniesByStatus", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@IsActive",status);
        await connection.OpenAsync();
        SqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            CompanyModel company = new CompanyModel();
            company.CompanyID = Convert.ToInt32(reader["CompanyID"]);
            company.MaxUser = Convert.ToInt32(reader["MaxUser"]);
            company.CompanyCode = reader["CompanyCode"].ToString();
            company.CompanyName = reader["CompanyName"].ToString();
            company.Email = reader["Email"].ToString();
            company.CompanyAdminId = Convert.ToInt32(reader["CompanyAdminId"]);
            company.CompanyImage = reader["CompanyImage"].ToString();
            company.CompanyFoundationDate = Convert.ToDateTime(reader["CompanyFoundationDate"]);
            company.BusinessRegistrationNumber = reader["BusinessRegistrationNumber"].ToString();
            company.TaxIdentificationNumber = reader["TaxIdentificationNumber"].ToString();
            company.TradeLicense = reader["TradeLicense"].ToString();
            company.PreferredPaymentMethodID = Convert.ToInt32(reader["PreferredPaymentMethodID"].ToString());
            company.PreferredPaymentMethodName = reader["PreferredPaymentMethodName"].ToString();
            company.BankNameID = Convert.ToInt32(reader["BankNameID"]);
            company.BankName = reader["BankName"].ToString();
            company.AccountNumber = reader["AccountNumber"].ToString();
            company.AccountHolderName = reader["AccountHolderName"].ToString();
            companies.Add(company);
        }
        connection.Close();
        return companies;
    }

    public async Task<string> UpdateCompanyAsync(CompanyDto companyDto)
    {


        string updateSql = "UPDATE CompanyRegistration SET ";
        if (companyDto.IsActive != null)
        {
            updateSql += " IsActive = @IsActive,";
        }
        if (companyDto.MaxUser != null)
        {
            updateSql += " MaxUser = @MaxUser,";
        }
        updateSql = updateSql.TrimEnd(',');
        updateSql += " WHERE CompanyCode = @CompanyCode;";
        SqlCommand cmd = new SqlCommand(updateSql, connection);
        if (companyDto.IsActive != null)
        {
            cmd.Parameters.AddWithValue("@IsActive", companyDto.IsActive);
        }
        if (companyDto.MaxUser != null)
        {
            cmd.Parameters.AddWithValue("@MaxUser", companyDto.MaxUser);
        }
        cmd.Parameters.AddWithValue("@CompanyCode", companyDto.CompanyCode);

        await connection.OpenAsync();
        // Execute the update
        int res = await cmd.ExecuteNonQueryAsync();
        await connection.CloseAsync();

        if (res > 0)
        {
            if (companyDto.IsActive != null)
            {
                if (companyDto.IsActive == 1)
                {
                    return "Company is Active Now.";
                }
                else if (companyDto.IsActive == 0)
                {
                    return "Company is InActive Now.";
                }

            }

        }
        return "there is a error";

    }
}
