using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Internal;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Internal;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Internal.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Internal.Responses;
using Aruba.CmpService.BaremetalProvider.Api.Code.ActionResults;
using Aruba.CmpService.BaremetalProvider.Api.Code.Controllers;
using Aruba.CmpService.BaremetalProvider.Api.Code.Filters;
using Aruba.CmpService.BaremetalProvider.Api.Code.Headers;
using Microsoft.AspNetCore.Mvc;

namespace Aruba.CmpService.BaremetalProvider.Api.v1.Controllers;

[Route("internal")]
public partial class InternalController : BaseController
{
    private readonly ILogger<FirewallsController> logger;

    public InternalController(ILogger<FirewallsController> logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// Get all resource of a users
    /// </summary>
    [HttpPost("resources")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<LegacyResourceResponseDto> Search([FromCeSource] string? source, GetResoucesDto model)
    {
        var request = new InternalGetResourcesUseCaseRequest()
        {
            UserId = User.GetUserId(),
            MessageBusRequestId = Guid.NewGuid().ToString(),
            Ids = model.Services,
            GetPrices = model.GetPrices,
        };

        return this.UseCase<InternalGetResourcesUseCaseRequest, InternalGetResourcesUseCaseResponse, IEnumerable<LegacyResource>>(request, o => o.MapToResponse()!);
    }

    /// <summary>
    /// Set autorenew
    /// </summary>
    [HttpPost("autorenew")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult Autorenew([FromCeSource] string? source, AutorenewDto autorenewDto)
    {
        var request = new InternalAutomaticRenewUseCaseRequest()
        {
            UserId = User.GetUserId(),
            MessageBusRequestId = Guid.NewGuid().ToString(),
            Resources = autorenewDto.Resources,
            RenewData = autorenewDto.AutoRenewData,
        };

        return this.UseCase<InternalAutomaticRenewUseCaseRequest, InternalAutomaticRenewUseCaseResponse>(request);
    }

    /// <summary>
    /// Get autorecharge info, using admin token
    /// </summary>
    [HttpGet("autorecharge/{userId}")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<AutorechargeResponse> Autorecharge([FromCeSource] string? source, string userId)
    {
        var request = new InternalAutorechargeUseCaseRequest()
        {
            UserId = userId,
            MessageBusRequestId = Guid.NewGuid().ToString(),
            UseAdminProvider = true,
        };

        return this.UseCase<InternalAutorechargeUseCaseRequest, InternalAutorechargeUseCaseResponse, AutorechargeResponse>(request, o => o);
    }

    /// <summary>
    /// Get all resource of a users, using admin token
    /// </summary>
    [HttpPost("resources/{userId}")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<BaseLegacyResourceResponseDto> Search([FromCeSource] string? source, string userId, IEnumerable<LegacyResourceIdDto>? model)
    {
        var request = new InternalAdminGetResourcesUseCaseRequest()
        {
            UserId = userId,
            MessageBusRequestId = Guid.NewGuid().ToString(),
            Ids = model
        };

        return this.UseCase<InternalAdminGetResourcesUseCaseRequest, InternalAdminGetResourcesUseCaseResponse, IEnumerable<BaseLegacyResource>>(request, o => o.MapToResponse()!);
    }

    /// <summary>
    /// Get regions
    /// </summary>
    [HttpGet("regions")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<IEnumerable<Region>> GetRegions([FromCeSource] string? source)
    {
        var request = new InternalGetRegionsUseCaseRequest()
        {
            MessageBusRequestId = Guid.NewGuid().ToString(),
        };

        return this.UseCase<InternalGetRegionsUseCaseRequest, InternalGetRegionsUseCaseResponse, IEnumerable<Region>>(request, o => o);
    }
}
