using NDE_Digital_Market.Services;
using NDE_Digital_Market.Model;
using NDE_Digital_Market.DTOs;
using NDE_Digital_Market.Data_Access_Layer;
using NDE_Digital_Market.SharedServices;

namespace NDE_Digital_Market.Services.CompanyRegistrationServices;

public class CompanyRegistration : ICompanyRegistration
{
    private readonly CompanyRegistration_DAL _CompanyRegistration_DAL;
    private readonly string foldername = "CompanyFiles"; 
    public CompanyRegistration(CompanyRegistration_DAL companyRegistration_DAL)
    {
        this._CompanyRegistration_DAL = companyRegistration_DAL;
    }


    public async Task<Boolean> CompanyexistsCheckAsync(CompanyDto companyDto)
    {
        Boolean res = await _CompanyRegistration_DAL.CompanyExistAsync(companyDto);
        return res;
    }


    public async Task<string> CompanyRegistrationPostAsync(CompanyDto companyDto)
    {
        string res = await _CompanyRegistration_DAL.CompanyRegistrationPostAsync(companyDto);
        return res;
    }

    public async Task<List<CompanyModel>> GetCompaniesAsync()
    {
        var res = await _CompanyRegistration_DAL.GetCompaniesAsync();
        return res;
    }
    public string UpdateCompany(UserModel userModel, CompanyModel companyModel)
    {
        var res = _CompanyRegistration_DAL.UpdateCompany(userModel, companyModel);
        return res;
    }
}
