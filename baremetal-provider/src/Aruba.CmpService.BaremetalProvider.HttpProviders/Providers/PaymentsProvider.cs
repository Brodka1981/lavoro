using System.Net;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Payments;
using Aruba.CmpService.BaremetalProvider.Abstractions.Utils;
using Aruba.CmpService.BaremetalProvider.HttpProviders.Extensions;
using Microsoft.Extensions.Logging;

namespace Aruba.CmpService.BaremetalProvider.HttpProviders.Providers;

public class PaymentsProvider : IPaymentsProvider
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<PaymentsProvider> logger;

    public PaymentsProvider(IHttpClientFactory httpClientFactory, ILogger<PaymentsProvider> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
    }

    /// <inheritdoc cref="IPaymentsProvider.GetPaymentMethodsAsync()"/>
    public async Task<ApiCallOutput<IEnumerable<LegacyPaymentMethod>>> GetPaymentMethodsAsync()
    {
        using var httpClient = this.CreateHttpClient();

        var response = await httpClient.CallGetAsync<IEnumerable<LegacyPaymentMethod>>("/common/api/payment/getcustomerdevices").ConfigureAwait(false);

        if (!response.Success
            || response.Result is null)
        {
            Log.LogWarning(logger, "Payment/GetCustomerDevices response: {response}", response.Serialize());

            return new ApiCallOutput<IEnumerable<LegacyPaymentMethod>>
            {
                StatusCode = HttpStatusCode.NotFound,
                Err = ApiError.New(HttpStatusCode.NotFound, "404", "payment methods not found", "GetCustomerDevices")
            };
        }

        response.Result = response.Result.Where(x => x.Status == LegacyPaymentStatus.Active).ToList();
        return response;

    }

    /// <inheritdoc cref="IPaymentsProvider.PayOrderAsync(LegacyPayOrderRequest)"/>
    public async Task<ApiCallOutput<LegacyPayOrderResponse>> PayOrderAsync(LegacyPayOrderRequest reqBody)
    {
        using var httpClient = this.CreateHttpClient();

        var response = await httpClient.CallPostAsync<LegacyPayOrderResponse>(
                "/common/api/payment/postdevicepayment",
                reqBody
            ).ConfigureAwait(false);

        if (!response.Success
            || response.Result is null)
        {
            Log.LogWarning(logger, "Payment/PostDevicePayment response: {response}", response.Serialize());

            return new ApiCallOutput<LegacyPayOrderResponse>
            {
                StatusCode = HttpStatusCode.NotFound,
                Err = ApiError.New(HttpStatusCode.InternalServerError, "500", "payment confirmation error", "PostDevicePayment")
            };
        }

        return response;
    }

    /// <inheritdoc cref="IPaymentsProvider.GetFraudRiskAssessmentAsync()"/>
    public async Task<ApiCallOutput<bool>> GetFraudRiskAssessmentAsync()
    {
        //// Fixed values for renew operations for Server, Switch and Firewall
        //var productTypeId = 4;
        //var serviceType = 36;
        //var orderType = 1;
        //var riskAssessmentServiceType = 0;

        //using var httpClient = this.CreateHttpClient();

        //var response = await httpClient
        //    .CallGetAsync<LegacyFraudRiskAssessmentResponse>($"/common/api/payment/getfraudriskassessment?producttypeid={productTypeId}&servicetype={serviceType}&ordertype={orderType}&riskassessmentservicetype={riskAssessmentServiceType}")
        //    .ConfigureAwait(false);

        //if (!response.Success)
        //{
        //    Log.LogWarning(logger, "Payment/GetFraudRiskAssessment response: {response}", response.Serialize());

        //    return new ApiCallOutput<bool>
        //    {
        //        StatusCode = HttpStatusCode.InternalServerError,
        //        Err = ApiError.New(HttpStatusCode.NotFound, "500", "Payment/GetFraudRiskAssessment error", "GetFraudRiskAssessment")
        //    };
        //}

        //return new ApiCallOutput<bool>
        //{
        //    Success = response.Success,
        //    StatusCode = HttpStatusCode.OK,
        //    Result = response.Result.Result,
        //};

        //2025-10-14 -- DEV-57276 - Requested bypass of fraud risk
        return new ApiCallOutput<bool>
        {
            Success = true,
            StatusCode = HttpStatusCode.OK,
            Result = false
        };
    }

    private HttpClient CreateHttpClient()
    {
        return this.httpClientFactory.CreateServiceClient(BaremetalHttpClientNames.LegacyProvider);
    }
}
