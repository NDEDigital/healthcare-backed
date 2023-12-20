using NDE_Digital_Market.Data_Access_Layer;
using NDE_Digital_Market.Model;
using System.Data.SqlClient;

namespace NDE_Digital_Market.Data_Access_Layer;

public class HK_Gets_DAL
{
    private readonly IConfiguration _configuration;
    private readonly SqlConnection con;
    public HK_Gets_DAL(IConfiguration configuration)
    {
        _configuration = configuration;
        con = new SqlConnection(_configuration.GetConnectionString("HealthCare"));
    }
    public async Task<List<PaymentMethodModel>> PaymentMethodGetAsync()
    {
        List<PaymentMethodModel> paymentMethods = new List<PaymentMethodModel>();
        SqlCommand command = new SqlCommand("select PMMasterID as PMID, PMName from HK_PaymentMethodMaster;", con);

        await con.OpenAsync();
        SqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            PaymentMethodModel paymentMethod = new PaymentMethodModel();
            paymentMethod.PMID = Convert.ToInt32(reader["PMID"]);
            paymentMethod.PMName = reader["PMName"].ToString();
            paymentMethods.Add(paymentMethod);
        }
        await con.CloseAsync();
        return paymentMethods;

    }

    public async Task<List<PaymentMethodModel>> BankNameGetAsync(int preferredPM)
    {
        List<PaymentMethodModel> paymentMethods = new List<PaymentMethodModel>();
        SqlCommand command = new SqlCommand("select PMDetailsID as PMID, PMBankName as PMName from HK_PaymentMethodDetails where PMMasterID = @preferredPM;", con);
        command.Parameters.AddWithValue("@preferredPM",preferredPM);
        await con.OpenAsync();
        SqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            PaymentMethodModel paymentMethod = new PaymentMethodModel();
            paymentMethod.PMID = Convert.ToInt32(reader["PMID"]);
            paymentMethod.PMName = reader["PMName"].ToString();
            paymentMethods.Add(paymentMethod);
        }
        await con.CloseAsync();
        return paymentMethods;

    }
}
