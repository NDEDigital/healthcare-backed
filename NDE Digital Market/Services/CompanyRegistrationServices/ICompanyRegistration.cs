
using NDE_Digital_Market.Model;
using NDE_Digital_Market.DTOs;

namespace NDE_Digital_Market.Services.CompanyRegistrationServices;

public interface ICompanyRegistration
{
    Task<string> CompanyRegistrationPostAsync(CompanyDto companyDto);
    Task<Boolean> CompanyexistsCheckAsync(CompanyDto companyDto);

    Task<List<CompanyModel>> GetCompaniesAsync();

    Task<string> UpdateCompanyAsync(CompanyDto companyDto);

}
