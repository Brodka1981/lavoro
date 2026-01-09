using System.Net;
using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos;
using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Switches;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Switches;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Switches.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Switches.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Switches.Responses;
using Aruba.CmpService.BaremetalProvider.Abstractions.Utils;
using Aruba.CmpService.BaremetalProvider.Api.Code.ActionResults;
using Aruba.CmpService.BaremetalProvider.Api.Code.Controllers;
using Aruba.CmpService.BaremetalProvider.Api.Code.Filters;
using Aruba.CmpService.BaremetalProvider.Api.Code.Headers;
using Aruba.CmpService.BaremetalProvider.Api.Code.Swagger;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery;
using Microsoft.AspNetCore.Mvc;
namespace Aruba.CmpService.BaremetalProvider.Api.v1.Controllers;

[Route("projects/{projectId}/providers/Aruba.Baremetal/switches")]
public partial class SwitchesController : BaseController
{
    private readonly ILogger<SwitchesController> logger;

    public SwitchesController(ILogger<SwitchesController> logger)
    {
        this.logger = logger;
    }

    #region WithProjects

    /// <summary>
    /// List switches
    /// </summary>
    [HttpGet()]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<SwitchListResponseDto> Search([FromCeSource] string? source, string projectId, [FromQuery] ResourceQueryDefinition query)
    {
        Log.LogDebug(logger, "filter passed to search {filters}", query.Serialize());
        var request = new SwitchSearchFilterRequest()
        {
            Query = query,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            External = this.IsExternalSource(source)
        };

        return this.QueryHandler<SwitchSearchFilterRequest, SwitchList>(request, o => o.MapToResponse(this.HttpContext.Request)!);
    }

    /// <summary>
    /// Get switch
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<SwitchResponseDto> Get([FromCeSource] string? source, string projectId, string id)
    {
        var request = new SwitchByIdRequest()
        {
            ResourceId = id,
            ProjectId = projectId,
            UserId = User.GetUserId(),
        };

        return this.QueryHandler<SwitchByIdRequest, Switch>(request, m => m.MapToResponse());
    }

    /// <summary>
    /// Rename switch
    /// </summary>
    [HttpPut("{id}/name")]
    [AllowAnonymous]
    [Produce200Family(HttpStatusCode.OK)]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult RenameAsync([FromCeSource] string? source, string projectId, string id, [FromBody] RenameDto model)
    {
        var request = new SwitchRenameUseCaseRequest()
        {
            ResourceId = id,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            MessageBusRequestId = Guid.NewGuid().ToString(),
            RenameData = model
        };

        return this.UseCase<SwitchRenameUseCaseRequest, SwitchRenameUseCaseResponse>(request);
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
        var request = new SwitchSetAutomaticRenewUseCaseRequest()
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

        return this.UseCase<SwitchSetAutomaticRenewUseCaseRequest, SwitchSetAutomaticRenewUseCaseResponse>(request);
    }
    #endregion

    #region NoProjects
    /// <summary>
    /// Servers catalog
    /// </summary>
    [HttpGet("~/providers/Aruba.Baremetal/switches/catalog")]
    [AllowAnonymous]
    public ActionResult<SwitchCatalogResponseDto> SearchCatalog([FromCeSource] string? source, [FromQuery] ResourceQueryDefinition query)
    {
        Log.LogDebug(logger, "filter passed to search {filters}", query.Serialize());
        var request = new CatalogFilterRequest()
        {
            Query = query,
            External = this.IsExternalSource(source)
        };

        return this.QueryHandler<CatalogFilterRequest, SwitchCatalog>(request, o => o.MapToResponse(this.HttpContext.Request)!);
    }
    #endregion
}
