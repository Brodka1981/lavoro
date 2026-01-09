using System.Net;
using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos;
using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Mcis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Servers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.MCIs;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.MCI;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.MCIs.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.MCIs.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.MCIs.Responses;
using Aruba.CmpService.BaremetalProvider.Abstractions.Utils;
using Aruba.CmpService.BaremetalProvider.Api.Code.ActionResults;
using Aruba.CmpService.BaremetalProvider.Api.Code.Controllers;
using Aruba.CmpService.BaremetalProvider.Api.Code.Filters;
using Aruba.CmpService.BaremetalProvider.Api.Code.Headers;
using Aruba.CmpService.BaremetalProvider.Api.Code.Swagger;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery;
using Microsoft.AspNetCore.Mvc;

namespace Aruba.CmpService.BaremetalProvider.Api.v1.Controllers;
[Route("projects/{projectId}/providers/Aruba.Baremetal/mcis")]
public partial class MCIsController : BaseController
{
    private readonly ILogger<MCIsController> logger;

    public MCIsController(ILogger<MCIsController> logger)
    {
        this.logger = logger;
    }

    #region WithProjects
    /// <summary>
    /// Get mcis matching filters
    /// </summary>
    [HttpGet()]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<MCIListResponseDto> Search([FromCeSource] string? source, string projectId, [FromQuery] ResourceQueryDefinition query)
    {
        Log.LogDebug(logger, "filter passed to search {filters}", query.Serialize());
        var request = new MCISearchFilterRequest()
        {
            Query = query,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            External = this.IsExternalSource(source)
        };

        return this.QueryHandler<MCISearchFilterRequest, MCIList>(request, o => o.MapToResponse(this.HttpContext.Request)!);
    }

    /// <summary>
    /// Get an existing mci by Id
    /// </summary>
    [HttpGet("{id}")]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<MCIResponseDto> Get([FromCeSource] string? source, string projectId, string id, [FromQuery] bool? calculatePrices = false)
    {
        var request = new MCIByIdRequest()
        {
            ResourceId = id,
            ProjectId = projectId,
            UserId = User.GetUserId(),
            CalculatePrices = calculatePrices ?? false
        };

        return this.QueryHandler<MCIByIdRequest, MCI>(request, m => m.MapToResponse());
    }

    /// <summary>
    /// Get an existing mci by Id
    /// </summary>
    [HttpGet("{id}/services")]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<MCIListContentResponseDto> GetServices([FromCeSource] string? source, string projectId, string id, [FromQuery] ResourceQueryDefinition query)
    {
        var request = new MCIContentByIdRequest()
        {
            ResourceId = id,
            ProjectId = projectId,
            UserId = User.GetUserId(),
            Query = query
        };

        return this.QueryHandler<MCIContentByIdRequest, MCI>(request, m => m.MapToListResponse());
    }

    /// <summary>
    /// Rename mci
    /// </summary>
    [HttpPut("{id}/name")]
    [Produce200Family(HttpStatusCode.OK)]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult Rename([FromCeSource] string? source, string projectId, string id, [FromBody] RenameDto model)
    {
        var request = new MCIRenameUseCaseRequest()
        {
            ResourceId = id,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            MessageBusRequestId = Guid.NewGuid().ToString(),
            RenameData = model
        };

        return this.UseCase<MCIRenameUseCaseRequest, MCIRenameUseCaseResponse>(request);
        //return MockActionOK();
    }

    /// <summary>
    /// Set automatic renew
    /// </summary>
    [HttpPut("{id}/automaticrenew")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    [Produce200Family(HttpStatusCode.OK)]
    public ActionResult SetAutomaticRenew([FromCeSource] string? source, string projectId, string id, SetAutomaticRenewDto model)
    {
        var request = new MCISetAutomaticRenewUseCaseRequest()
        {
            ResourceId = id,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            MessageBusRequestId = Guid.NewGuid().ToString(),
            PaymentMethodId = model.PaymentMethodId,
            Months = model.Months,
            ActionOnFolder = model.ActionOnFolder,
            Activate = model.Activate,
        };
        return this.UseCase<MCISetAutomaticRenewUseCaseRequest, MCISetAutomaticRenewUseCaseResponse>(request);
        //return MockActionOK();
    }

    #endregion

    #region NoProjects
    /// <summary>
    /// MCI catalog
    /// </summary>
    [HttpGet("~/providers/Aruba.Baremetal/mcis/catalog")]
    [AllowAnonymous]
    public ActionResult<ServerCatalogResponseDto> SearchCatalog([FromCeSource] string? source, [FromQuery] ResourceQueryDefinition query, [FromQuery] string? language)
    {
        Log.LogDebug(logger, "filter passed to search {filters}", query.Serialize());
        var request = new CatalogFilterRequest()
        {
            Query = query,
            External = this.IsExternalSource(source),
            Language = language
        };

        return this.QueryHandler(request, (Abstractions.Models.MCIs.MCICatalog o) => o.MapToResponse()!);
    }

    #endregion

    #region MOCKS

    private ActionResult MockActionOK()
    {
        return Ok();
    }

    #endregion
}
