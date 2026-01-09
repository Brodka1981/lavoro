using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Payments;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;

public interface IPaymentsProvider
{
    /// <summary>
    /// Get payment methods list
    /// </summary>
    Task<ApiCallOutput<IEnumerable<LegacyPaymentMethod>>> GetPaymentMethodsAsync();

    /// <summary>
    /// Confirm order payment.
    /// </summary>
    Task<ApiCallOutput<LegacyPayOrderResponse>> PayOrderAsync(LegacyPayOrderRequest reqBody);

    /// <summary>
    /// Evaluate fraud risk
    /// </summary>
    /// <returns>TRUE if there is risk</returns>
    Task<ApiCallOutput<bool>> GetFraudRiskAssessmentAsync();
}
