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
    private readonly IConfiguration _configuration;
    private readonly UserController _UserController;
    private readonly SqlConnection connection;
    public CompanyRegistration_DAL(IConfiguration configuration, UserController userController)
    {
        _configuration = configuration;
        connection = new SqlConnection(_configuration.GetConnectionString("HealthCare"));
        _UserController = userController;
    }
    public async Task<Boolean> CompanyExistAsync(CompanyDto companyDto)
    {
        SqlCommand cmd = new SqlCommand("CheckCompanyExistence", connection);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@CompanyName", companyDto.CompanyName);
        cmd.Parameters.AddWithValue("@BusinessRegistrationNumber", companyDto.BusinessRegistrationNumber);
        cmd.Parameters.AddWithValue("@TaxIdentificationNumber", companyDto.TaxIdentificationNumber);
        await connection.OpenAsync();
        int count = (int) await cmd.ExecuteScalarAsync();
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
            return "company is exists";
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

            int CompanyID = int.Parse(systemCode.Split('%')[0]);
            string CompanyCode = systemCode.Split('%')[1];
            //SP END

            SqlCommand cmd = new SqlCommand("INSERT INTO CompanyRegistration (CompanyID, CompanyCode, CompanyName, CompanyImage, " +
                           "CompanyFoundationDate, BusinessRegistrationNumber, TaxIdentificationNumber, " +
                           "TradeLicense, PreferredPaymentMethodID, BankNameID, AccountNumber, " +
                           "AccountHolderName,MaxUser,IsActive, AddedBy, DateAdded, AddedPC) " +
                           "VALUES (@CompanyID, @CompanyCode, @CompanyName, @CompanyImage, " +
                           "@CompanyFoundationDate, @BusinessRegistrationNumber, @TaxIdentificationNumber, " +
                           "@TradeLicense, @PreferredPaymentMethodID, @BankNameID, @AccountNumber, " +
                           "@AccountHolderName, @MaxUser, @IsActive, @AddedBy, @DateAdded, @AddedPC);", connection);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@CompanyID", CompanyID);
            cmd.Parameters.AddWithValue("@CompanyCode", CompanyCode);
            cmd.Parameters.AddWithValue("@CompanyName", companyDto.CompanyName);
            cmd.Parameters.AddWithValue("@CompanyImage", companyDto.CompanyImage);
            cmd.Parameters.AddWithValue("@CompanyFoundationDate", companyDto.CompanyFoundationDate);
            cmd.Parameters.AddWithValue("@BusinessRegistrationNumber", companyDto.BusinessRegistrationNumber);
            cmd.Parameters.AddWithValue("@TaxIdentificationNumber", companyDto.TaxIdentificationNumber);
            cmd.Parameters.AddWithValue("@TradeLicense", companyDto.TradeLicense);
            cmd.Parameters.AddWithValue("@PreferredPaymentMethodID", companyDto.PreferredPaymentMethodID);
            cmd.Parameters.AddWithValue("@BankNameID", companyDto.BankNameID);
            cmd.Parameters.AddWithValue("@AccountNumber", companyDto.AccountNumber);
            cmd.Parameters.AddWithValue("@AccountHolderName", companyDto.AccountHolderName);
            cmd.Parameters.AddWithValue("@MaxUser", 3);
            cmd.Parameters.AddWithValue("@IsActive", 0);
            cmd.Parameters.AddWithValue("@AddedBy", companyDto.AddedBy);
            cmd.Parameters.AddWithValue("@DateAdded", DateTime.Now);
            cmd.Parameters.AddWithValue("@AddedPC", companyDto.AddedPC);

            await connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await connection.CloseAsync();

            return "Company Registration successfully.";
        }



    }

    public async Task<List<CompanyModel>> GetCompaniesAsync()
    {
        List<CompanyModel> companies = new List<CompanyModel>();
        SqlCommand command = new SqlCommand("GetCompanies", connection);
        command.CommandType = CommandType.StoredProcedure;

        await connection.OpenAsync();
        SqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            CompanyModel company = new CompanyModel();
            company.CompanyID = Convert.ToInt32(reader["CompanyID"]);
            company.CompanyCode = reader["CompanyCode"].ToString();
            company.CompanyName = reader["CompanyName"].ToString();
            company.CompanyAdminCode = reader["CompanyAdminCode"].ToString();
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

    public string UpdateCompany(UserModel userModel, CompanyModel companyModel)
    {
        ////var result = await _UserController.CreateUser(userModel);

        ////// Accessing usercode property
        ////string usercode = ((ObjectResult)result).Value.GetType().GetProperty("usercode")?.GetValue(((ObjectResult)result).Value)?.ToString();


        //string updateSql = "UPDATE CompanyRegistration SET ";
        //if (companyModel.IsActive != null)
        //{
        //    updateSql += " IsActive = @IsActive,";
        //}
        //if (companyModel.CompanyAdminCode != null)
        //{
        //    updateSql += " CompanyAdminCode = @CompanyAdminCode,";
        //}
        ////if (res.message != null)
        ////{
        ////    updateSql += " CompanyAdminCode = @CompanyAdminCode,";
        ////}
        //updateSql = updateSql.TrimEnd(',');
        //updateSql += " WHERE CompanyCode = @CompanyCode;";
        //SqlCommand cmd = new SqlCommand(updateSql, connection);
        //if (companyModel.IsActive != null)
        //{
        //    cmd.Parameters.AddWithValue("@IsActive", companyModel.IsActive);
        //}
        //if (companyModel.CompanyAdminCode != null)
        //{
        //    cmd.Parameters.AddWithValue("@CompanyAdminCode", companyModel.CompanyAdminCode);
        //}
        //cmd.Parameters.AddWithValue("@CompanyCode", companyModel.CompanyCode);

        //connection.Open();
        //// Execute the update
        //int res = cmd.ExecuteNonQuery();
        //connection.Close();

        //if (res > 0)
        //{
        //    return "Company is Active Now";
        //}
        return "there is a error";

    }
}
