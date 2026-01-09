using Aruba.CmpService.BaremetalProvider.Abstractions.Constants;
using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Payments;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Payments;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Payments;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Payments.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.Utils;
using Microsoft.Extensions.Logging;

namespace Aruba.CmpService.BaremetalProvider.Infrastructure.Services;

public class PaymentsService :
    IPaymentsService
{
    private readonly IPaymentsProvider paymentProvider;
    private readonly ILogger<PaymentsService> logger;
    private readonly IProfileProvider profileProvider;

    public PaymentsService(
        IPaymentsProvider paymentProvider,
        ILogger<PaymentsService> logger,
        IProfileProvider profileProvider)
    {
        this.paymentProvider = paymentProvider;
        this.logger = logger;
        this.profileProvider = profileProvider;
    }

    public async Task<ServiceResult<IEnumerable<PaymentMethod>>> GetPaymentMethodsAsync(PaymentMethodsAllRequest request, CancellationToken cancellationToken)
    {
        var legacyFraudRiskAssessmentResponse = await paymentProvider.GetFraudRiskAssessmentAsync().ConfigureAwait(false);
        if (!legacyFraudRiskAssessmentResponse.Success)
        {
            Log.LogWarning(logger, "GetPaymentMethodsAsync error: {legacyResponse}", legacyFraudRiskAssessmentResponse.Serialize());
            return ServiceResult<IEnumerable<PaymentMethod>>.CreateInternalServerError();
        }

        var legacyPaymentMethods = new List<LegacyPaymentMethod>();
        // Fraud risk assessment returns false if there is no risk   
        if (!legacyFraudRiskAssessmentResponse.Result)
        {
            var legacyPaymentMethodsResponse = await paymentProvider.GetPaymentMethodsAsync().ConfigureAwait(false);
            if (!legacyPaymentMethodsResponse.Success || legacyPaymentMethodsResponse.Result is null)
            {
                Log.LogWarning(logger, "GetPaymentMethodsAsync error: {legacyResponse}", legacyPaymentMethodsResponse.Serialize());
                return ServiceResult<IEnumerable<PaymentMethod>>.CreateInternalServerError();
            }

            legacyPaymentMethods = legacyPaymentMethodsResponse.Result.ToList();
        }

        var ret = new ServiceResult<IEnumerable<PaymentMethod>>()
        {
            Value = legacyPaymentMethods!.Select(p => p.MapToListItem()).ToList(),
        };

        return ret;
    }

    public async Task<ServiceResult<PaymentMethod>> GetPaymentMethodByIdAsync(PaymentMethodByIdRequest request, CancellationToken cancellationToken)
    {
        var allRequest = new PaymentMethodsAllRequest()
        {
            UserId = request.UserId,
        };
        var payments = await this.GetPaymentMethodsAsync(allRequest, cancellationToken).ConfigureAwait(false);
        if (payments.Value != null)
        {
            return new ServiceResult<PaymentMethod>()
            {
                Value = payments.Value.FirstOrDefault(f => f.Id.Equals(request.Id, StringComparison.OrdinalIgnoreCase))
            };
        }
        return ServiceResult<PaymentMethod>.Clone(payments);
    }

    public async Task<string> GetLegacyProviderAsync(string userId)
    {
        var profileResponse = await this.profileProvider.GetUser(userId).ConfigureAwait(false);

        if (profileResponse.Success
            && profileResponse.Result != null
            && (profileResponse.Result.IsResellerCustomer ?? false))
        {
            return LegacyPaymentProviderType.CloudReseller.Value;
        }

        return LegacyPaymentProviderType.Cloud.Value;
    }

    /// <inheritdoc cref="IPaymentsService.PayOrderAsync(PayOrderRequest, CancellationToken)">
    public async Task<ServiceResult<PayOrderResponse>> PayOrderWithWalletAsync(string userId, PayOrderRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var legacyPaymentProvider = await GetLegacyProviderAsync(userId).ConfigureAwait(false);

        // If user type is cloud reseller, the order has been already paid at the moment of creation; nothing to do here
        if (legacyPaymentProvider == LegacyPaymentProviderType.CloudReseller.Value) {
            return new ServiceResult<PayOrderResponse>()
            {
                Value = new PayOrderResponse()
                {
                    Success = true
                }
            };
        }

        var legacyPaymentMethodsResponse = await paymentProvider.GetPaymentMethodsAsync().ConfigureAwait(false);
        if (!legacyPaymentMethodsResponse.Success || legacyPaymentMethodsResponse.Result is null)
        {
            Log.LogWarning(logger, "GetPaymentMethodsAsync error: {legacyResponse}", legacyPaymentMethodsResponse.Serialize());
            return ServiceResult<PayOrderResponse>.CreateInternalServerError();
        }

        var legacyPaymentMethods = legacyPaymentMethodsResponse.Result.ToList();
        var walletPaymentMethod = legacyPaymentMethods
            .FirstOrDefault(p => p.DeviceType == LegacyPaymentType.Wallet);
        if (walletPaymentMethod == null)
        {
            Log.LogWarning(logger, "{methodName} error: unable to find a wallet device payment for the current user", nameof(PayOrderWithWalletAsync));
            return ServiceResult<PayOrderResponse>.CreateInternalServerError();
        }

        var legacyPayOrderResponse = await paymentProvider
            .PayOrderAsync(new LegacyPayOrderRequest()
            {
                CustomInfo = [],
                DeviceID = walletPaymentMethod.Id,
                DeviceType = walletPaymentMethod.DeviceType,
                OrderId = request.OrderId,
                PARes = null,
                ReferenceTransaction = null,
                ReturnURL = null,
                RiskAssessmentOperationId = null,
                ServiceType = request.ServiceType,
            })
            .ConfigureAwait(false);
        if (!legacyPayOrderResponse.Success || legacyPayOrderResponse.Result is null)
        {
            Log.LogWarning(logger, "{methodName} error: {legacyResponse}", nameof(PayOrderWithWalletAsync), legacyPayOrderResponse.Serialize());
            return ServiceResult<PayOrderResponse>.CreateInternalServerError();
        }

        return new ServiceResult<PayOrderResponse>()
        {
            Value = new PayOrderResponse()
            {
                Success = true
            }
        };
    }
}
