using NDE_Digital_Market.Model;

namespace NDE_Digital_Market.Services.HK_GetsServices
{
    public interface IHK_Gets
    {
        Task<List<PaymentMethodModel>> PaymentMethodGetAsync();
        Task<List<PaymentMethodModel>> BankNameGetAsync(int preferredPM);
    }
}
