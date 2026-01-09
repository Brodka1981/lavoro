using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Payments;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Payments.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;

public interface IPaymentsService
{
    Task<ServiceResult<IEnumerable<PaymentMethod>>> GetPaymentMethodsAsync(PaymentMethodsAllRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<PaymentMethod>> GetPaymentMethodByIdAsync(PaymentMethodByIdRequest request, CancellationToken cancellationToken);
    Task<string> GetLegacyProviderAsync(string userId);

    /// <summary>
    /// @hpc-jan-2026
    /// NOTE: for HPC implementation by the end of January 2026. This method is supposed to handle the payment of an order
    /// only by reseller or via wallet credit.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ServiceResult<PayOrderResponse>> PayOrderWithWalletAsync(string userId, PayOrderRequest request, CancellationToken cancellationToken);

}
