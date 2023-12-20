
using NDE_Digital_Market.Model;
using NDE_Digital_Market.Data_Access_Layer;

namespace NDE_Digital_Market.Services.HK_GetsServices;

public class HK_Gets : IHK_Gets
{
    private readonly HK_Gets_DAL _HK_Gets_DAL;
    public HK_Gets(HK_Gets_DAL hK_Gets_DAL)
    {
        this._HK_Gets_DAL = hK_Gets_DAL;
    }
    public async Task<List<PaymentMethodModel>> PaymentMethodGetAsync()
    {
        List<PaymentMethodModel> res = await _HK_Gets_DAL.PaymentMethodGetAsync();
        return res;
    }
    public async Task<List<PaymentMethodModel>> BankNameGetAsync(int preferredPM)
    {
        List<PaymentMethodModel> res = await _HK_Gets_DAL.BankNameGetAsync(preferredPM);
        return res;
    }
}
