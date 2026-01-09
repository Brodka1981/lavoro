using System.Net;
using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos;
using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Servers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Servers;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Servers.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Servers.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Servers.Responses;
using Aruba.CmpService.BaremetalProvider.Abstractions.Utils;
using Aruba.CmpService.BaremetalProvider.Api.Code.ActionResults;
using Aruba.CmpService.BaremetalProvider.Api.Code.Controllers;
using Aruba.CmpService.BaremetalProvider.Api.Code.Filters;
using Aruba.CmpService.BaremetalProvider.Api.Code.Headers;
using Aruba.CmpService.BaremetalProvider.Api.Code.Swagger;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Swaases;

namespace Aruba.CmpService.BaremetalProvider.Api.v1.Controllers;

[Route("projects/{projectId}/providers/Aruba.Baremetal/servers")]
public partial class ServersController : BaseController
{
    private readonly ILogger<ServersController> logger;
    private readonly BaremetalOptions baremetalOptions;

    public ServersController(ILogger<ServersController> logger, IOptions<BaremetalOptions> baremetalOptions)
    {
        this.logger = logger;
        this.baremetalOptions = baremetalOptions.Value;
    }

    #region WithProjects

    /// <summary>
    /// List servers
    /// </summary>
    [HttpGet()]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<ServerListResponseDto> Search([FromCeSource] string? source, string projectId, [FromQuery] ResourceQueryDefinition query)
    {
        Log.LogDebug(logger, "filter passed to search {filters}", query.Serialize());
        var request = new ServerSearchFilterRequest()
        {
            Query = query,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            External = this.IsExternalSource(source)
        };

        return this.QueryHandler<ServerSearchFilterRequest, ServerList>(request, o => o.MapToResponse(this.HttpContext.Request)!);
    }

    /// <summary>
    /// Get server
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<ServerResponseDto> Get([FromCeSource] string? source, string projectId, string id)
    {
        var request = new ServerByIdRequest()
        {
            ResourceId = id,
            ProjectId = projectId,
            UserId = User.GetUserId(),
        };

        return this.QueryHandler<ServerByIdRequest, Server>(request, m => m.MapToResponse()!);
    }

    /// <summary>
    /// Restart server
    /// </summary>
    [HttpPut("{id}/restart")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult Restart([FromCeSource] string? source, string projectId, string id)
    {
        var request = new ServerRestartUseCaseRequest()
        {
            ResourceId = id,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            MessageBusRequestId = Guid.NewGuid().ToString(),
        };

        return this.UseCase<ServerRestartUseCaseRequest, ServerRestartUseCaseResponse>(request);
    }

    /// <summary>
    /// Rename server
    /// </summary>
    [HttpPut("{id}/name")]
    [AllowAnonymous]
    [Produce200Family(HttpStatusCode.OK)]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult Rename([FromCeSource] string? source, string projectId, string id, [FromBody] RenameDto model)
    {
        var request = new ServerRenameUseCaseRequest()
        {
            ResourceId = id,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            MessageBusRequestId = Guid.NewGuid().ToString(),
            RenameData = model
        };

        return this.UseCase<ServerRenameUseCaseRequest, ServerRenameUseCaseResponse>(request);
    }

    /// <summary>
    /// List server Ips
    /// </summary>
    [HttpGet("{id}/ips")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<ServerIpAddressList> SearchIpAddress([FromCeSource] string? source, long id, string projectId, [FromQuery] ResourceQueryDefinition query)
    {
        Log.LogDebug(logger, "filter passed to search {filters}", query.Serialize());
        var request = new ServerIpAddressesFilterRequest()
        {
            Query = query,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            ResourceId = id,
            External = this.IsExternalSource(source)
        };

        return this.QueryHandler<ServerIpAddressesFilterRequest, ServerIpAddressList>(request, o => o.MapToResponse(this.HttpContext.Request)!);
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
        var request = new ServerUpdateIpUseCaseRequest()
        {
            ResourceId = id,
            Id = ipId,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            MessageBusRequestId = Guid.NewGuid().ToString(),
            IpAddress = model
        };

        return this.UseCase<ServerUpdateIpUseCaseRequest, ServerUpdateIpUseCaseResponse>(request);
    }

    /// <summary>
    /// Delete plesk license
    /// </summary>
    [HttpDelete("{id}/pleskLicense")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    [Produce200Family(HttpStatusCode.OK)]
    public ActionResult DeletePleskLicense([FromCeSource] string? source, string projectId, string id)
    {
        var request = new DeletePleskLicenseRequest()
        {
            ResourceId = id,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        return this.UseCase<DeletePleskLicenseRequest, DeletePleskLicenseResponse>(request);
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
        var request = new ServerSetAutomaticRenewUseCaseRequest()
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

        return this.UseCase<ServerSetAutomaticRenewUseCaseRequest, ServerSetAutomaticRenewUseCaseResponse>(request);
    }
    #endregion

    #region NoProjects
    /// <summary>
    /// Servers catalog
    /// </summary>
    [HttpGet("~/providers/Aruba.Baremetal/servers/catalog")]
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

        return this.QueryHandler(request, (Abstractions.Models.Servers.ServerCatalog o) => o.MapToResponse(this.HttpContext.Request)!);
    }
    /// <summary>
    /// Servers catalog
    /// </summary>
    [HttpGet("~/providers/Aruba.Baremetal/servers/metadata")]
    [AllowAnonymous]
    public ActionResult<ServerMetadataResponseDto> GetMetadata([FromCeSource] string? source)
    {
        return new ServerMetadataResponseDto()
        {
            FrameworkAiGuides = this.baremetalOptions.FrameworkAiGuides
        };
    }
    #endregion
}
