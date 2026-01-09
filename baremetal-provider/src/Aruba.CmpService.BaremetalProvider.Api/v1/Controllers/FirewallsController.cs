using System.Net;
using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos;
using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Firewalls;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Firewalls;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Firewalls.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Firewalls.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Firewalls.Responses;
using Aruba.CmpService.BaremetalProvider.Abstractions.Utils;
using Aruba.CmpService.BaremetalProvider.Api.Code.ActionResults;
using Aruba.CmpService.BaremetalProvider.Api.Code.Controllers;
using Aruba.CmpService.BaremetalProvider.Api.Code.Filters;
using Aruba.CmpService.BaremetalProvider.Api.Code.Headers;
using Aruba.CmpService.BaremetalProvider.Api.Code.Swagger;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery;
using Microsoft.AspNetCore.Mvc;

namespace Aruba.CmpService.BaremetalProvider.Api.v1.Controllers;

[Route("projects/{projectId}/providers/Aruba.Baremetal/firewalls")]
public partial class FirewallsController : BaseController
{
    private readonly ILogger<FirewallsController> logger;

    public FirewallsController(ILogger<FirewallsController> logger)
    {
        this.logger = logger;
    }

    #region WithProjects

    /// <summary>
    /// Get firewalls matching filters
    /// </summary>
    [HttpGet()]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<FirewallListResponseDto> Search([FromCeSource] string? source, string projectId, [FromQuery] ResourceQueryDefinition query)
    {
        Log.LogDebug(logger, "filter passed to search {filters}", query.Serialize());
        var request = new FirewallSearchFilterRequest()
        {
            Query = query,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            External = this.IsExternalSource(source)
        };

        return this.QueryHandler<FirewallSearchFilterRequest, FirewallList>(request, o => o.MapToResponse(this.HttpContext.Request)!);
    }

    /// <summary>
    /// Get an existing firewall by Id
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<FirewallResponseDto> Get([FromCeSource] string? source, string projectId, string id)
    {
        var request = new FirewallByIdRequest()
        {
            ResourceId = id,
            ProjectId = projectId,
            UserId = User.GetUserId(),
        };

        return this.QueryHandler<FirewallByIdRequest, Firewall>(request, m => m.MapToResponse());
    }

    /// <summary>
    /// Get vlanids for a firewall by Id
    /// </summary>
    [HttpGet("{id}/vlanids")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<FirewallVlanResponseDto> GetVlanIDs([FromCeSource] string? source, string projectId, long id, [FromQuery] ResourceQueryDefinition query)
    {
        var request = new FirewallVlanIdFilterRequest()
        {
            ResourceId = id,
            ProjectId = projectId,
            UserId = User.GetUserId(),
            Query = query
        };

        return this.QueryHandler<FirewallVlanIdFilterRequest, FirewallVlanIdList>(request, m => m.MapToVlanIdResponse(this.HttpContext.Request)!);
    }

    /// <summary>
    /// Rename firewall
    /// </summary>
    [HttpPut("{id}/name")]
    [AllowAnonymous]
    [Produce200Family(HttpStatusCode.OK)]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult Rename([FromCeSource] string? source, string projectId, string id, [FromBody] RenameDto model)
    {
        var request = new FirewallRenameUseCaseRequest()
        {
            ResourceId = id,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            MessageBusRequestId = Guid.NewGuid().ToString(),
            RenameData = model
        };

        return this.UseCase<FirewallRenameUseCaseRequest, FirewallRenameUseCaseResponse>(request);
    }

    /// <summary>
    /// List firewall Ips
    /// </summary>
    [HttpGet("{id}/ips")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<FirewallIpAddressList> SearchIpAddress([FromCeSource] string? source, long id, string projectId, [FromQuery] ResourceQueryDefinition query)
    {
        Log.LogDebug(logger, "filter passed to search {filters}", query.Serialize());
        var request = new FirewallIpAddressesFilterRequest()
        {
            Query = query,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            ResourceId = id,
            External = this.IsExternalSource(source)
        };

        return this.QueryHandler<FirewallIpAddressesFilterRequest, FirewallIpAddressList>(request, o => o.MapToResponse(this.HttpContext.Request)!);
    }

    /// <summary>
    /// Update Ip
    /// </summary>
    [HttpPut("{id}/ips/{ipId}")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    [Produce200Family(HttpStatusCode.OK)]
    public ActionResult UpdateIp(string projectId, string id, long ipId, [FromBody] IpAddressDto model)
    {
        var request = new FirewallUpdateIpUseCaseRequest()
        {
            ResourceId = id,
            Id = ipId,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            MessageBusRequestId = Guid.NewGuid().ToString(),
            IpAddress = model
        };

        return this.UseCase<FirewallUpdateIpUseCaseRequest, FirewallUpdateIpUseCaseResponse>(request);
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
        var request = new FirewallSetAutomaticRenewUseCaseRequest()
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

        return this.UseCase<FirewallSetAutomaticRenewUseCaseRequest, FirewallSetAutomaticRenewUseCaseResponse>(request);
    }
    #endregion

    #region NoProjects
    /// <summary>
    /// Servers catalog
    /// </summary>
    [HttpGet("~/providers/Aruba.Baremetal/firewalls/catalog")]
    [AllowAnonymous]
    public ActionResult<FirewallCatalogResponseDto> SearchCatalog([FromCeSource] string? source, [FromQuery] ResourceQueryDefinition query)
    {
        Log.LogDebug(logger, "filter passed to search {filters}", query.Serialize());
        var request = new CatalogFilterRequest()
        {
            Query = query,
            External = this.IsExternalSource(source)
        };

        return this.QueryHandler<CatalogFilterRequest, FirewallCatalog>(request, o => o.MapToResponse(this.HttpContext.Request)!);
    }
    #endregion
}
