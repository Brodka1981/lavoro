using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Payments;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Internal;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Payments;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Payments.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Internal.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Internal.Responses;
using Aruba.CmpService.BaremetalProvider.Api.Code.ActionResults;
using Aruba.CmpService.BaremetalProvider.Api.Code.Controllers;
using Aruba.CmpService.BaremetalProvider.Api.Code.Filters;
using Aruba.CmpService.BaremetalProvider.Api.Code.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Aruba.CmpService.BaremetalProvider.Api.v1.Controllers;

[Route("providers/Aruba.Baremetal/payments")]
public class PaymentsController : BaseController
{
    private readonly ILogger<PaymentsController> logger;
    private readonly RenewFrequencyOptions renewFrequencyOptions;
    private readonly BaremetalOptions baremetalOptions;
    private readonly IPaymentsService paymentsService;

    public PaymentsController(
        ILogger<PaymentsController> logger,
        IOptions<RenewFrequencyOptions> renewFrequencyOptions,
        IOptions<BaremetalOptions> baremetalOptions,
        IPaymentsService paymentsService)
    {
        this.logger = logger;
        this.renewFrequencyOptions = renewFrequencyOptions?.Value ?? new RenewFrequencyOptions();
        this.baremetalOptions = baremetalOptions?.Value ?? new BaremetalOptions();
        this.paymentsService = paymentsService;
    }

    /// <summary>
    /// Get renew frequency
    /// </summary>
    [HttpGet("renewFrequency")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<RenewFrequencyResponseDto> GetRenewFrequency([FromCeSource] string? source)
    {
        var ret = new RenewFrequencyResponseDto(renewFrequencyOptions);
        return ret;
    }

    /// <summary>
    /// Get payment methods
    /// </summary>
    [HttpGet("methods")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<PaymentMethodsResponseDto> GetPaymentMethods([FromCeSource] string? source)
    {
        var request = new PaymentMethodsAllRequest()
        {
            UserId = User.GetUserId()
        };

        return this.QueryHandler<PaymentMethodsAllRequest, IEnumerable<PaymentMethod>>(request, v => v.MapToResponse());
    }


    /// <summary>
    /// Get payment method 
    /// </summary>
    [HttpGet("methods/{id}")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<PaymentMethodResponseDto> GetPaymentMethod([FromCeSource] string? source, string id)
    {
        var request = new PaymentMethodByIdRequest()
        {
            UserId = User.GetUserId(),
            Id = id
        };

        return this.QueryHandler<PaymentMethodByIdRequest, PaymentMethod>(request, v => v.MapToResponse());
    }

    /// <summary>
    /// Get metadata
    /// </summary>
    [HttpGet("metadata")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public async Task<ActionResult<PaymentMetadataResponseDto>> GetMetadata([FromCeSource] string? source)
    {
        return new PaymentMetadataResponseDto
        {
            PurchaseUrl = baremetalOptions.PurchaseUrl,
            RenewAndUpgradeUrl = baremetalOptions.RenewAndUpgradeUrl,
            RenewAndUpgradeSmartStorageAndSwaasUrl = baremetalOptions.RenewAndUpgradeSmartStorageAndSwaasUrl,
            RenewUrl = baremetalOptions.RenewUrl,
            Provider = await this.paymentsService.GetLegacyProviderAsync(User.GetUserId()).ConfigureAwait(false),
            SddTimeLimit = baremetalOptions.SddTimeLimit,
        };
    }


    /// <summary>
    /// Get autorecharge info
    /// </summary>
    [HttpGet("autorecharge")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<AutorechargeResponse> Autorecharge([FromCeSource] string? source)
    {
        var request = new InternalAutorechargeUseCaseRequest()
        {
            UserId = User.GetUserId(),
            MessageBusRequestId = Guid.NewGuid().ToString(),
        };

        return this.UseCase<InternalAutorechargeUseCaseRequest, InternalAutorechargeUseCaseResponse, AutorechargeResponse>(request, o => o);
    }

}