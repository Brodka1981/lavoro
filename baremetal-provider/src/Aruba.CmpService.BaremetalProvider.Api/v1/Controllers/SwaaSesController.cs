using System.Net;
using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos;
using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Swaases;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SwaaSes;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Swaases.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Swaases.Responses;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SwaaSes.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SwaaSes.Responses;
using Aruba.CmpService.BaremetalProvider.Abstractions.Utils;
using Aruba.CmpService.BaremetalProvider.Api.Code.ActionResults;
using Aruba.CmpService.BaremetalProvider.Api.Code.Controllers;
using Aruba.CmpService.BaremetalProvider.Api.Code.Filters;
using Aruba.CmpService.BaremetalProvider.Api.Code.Headers;
using Aruba.CmpService.BaremetalProvider.Api.Code.Swagger;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery;
using Microsoft.AspNetCore.Mvc;

namespace Aruba.CmpService.BaremetalProvider.Api.v1.Controllers;

[Route("projects/{projectId}/providers/Aruba.Baremetal/swaases")]
public partial class SwaasesController : BaseController
{
    private readonly ILogger<SwaasesController> logger;

    public SwaasesController(ILogger<SwaasesController> logger)
    {
        this.logger = logger;
    }

    #region WithProjects

    /// <summary>
    /// Search switch as a service
    /// </summary>
    [HttpGet()]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<SwaasListResponseDto> Search([FromCeSource] string? source, string projectId, [FromQuery] ResourceQueryDefinition query)
    {
        Log.LogDebug(logger, "filter passed to search {filters}", query.Serialize());
        var request = new SwaasSearchFilterRequest()
        {
            Query = query,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            External = this.IsExternalSource(source)
        };

        return this.QueryHandler<SwaasSearchFilterRequest, SwaasList>(request, o => o.MapToResponse(this.HttpContext.Request)!);
    }

    /// <summary>
    /// Get switch as a service 
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<SwaasResponseDto> Get([FromCeSource] string? source, string projectId, string id)
    {
        var request = new SwaasByIdRequest()
        {
            ResourceId = id,
            ProjectId = projectId,
            UserId = User.GetUserId(),
        };

        return this.QueryHandler<SwaasByIdRequest, Swaas>(request, m => m.MapToResponse());
    }


    /// <summary>
    /// Rename switch as a service
    /// </summary>
    [HttpPut("{id}/name")]
    [AllowAnonymous]
    [Produce200Family(HttpStatusCode.OK)]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult RenameAsync([FromCeSource] string? source, string projectId, string id, [FromBody] RenameDto model)
    {
        var request = new SwaasRenameUseCaseRequest()
        {
            ResourceId = id,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            MessageBusRequestId = Guid.NewGuid().ToString(),
            RenameData = model
        };

        return this.UseCase<SwaasRenameUseCaseRequest, SwaasRenameUseCaseResponse>(request);
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
        var request = new SwaasSetAutomaticRenewUseCaseRequest()
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

        return this.UseCase<SwaasSetAutomaticRenewUseCaseRequest, SwaasSetAutomaticRenewUseCaseResponse>(request);
    }
    #endregion

    #region NoProjects

    /// <summary>
    /// Swaases catalog
    /// </summary>
    [HttpGet("~/providers/Aruba.Baremetal/swaases/catalog")]
    [AllowAnonymous]
    public ActionResult<SwaasCatalogResponseDto> SearchCatalog([FromCeSource] string? source, [FromQuery] ResourceQueryDefinition query, [FromQuery] string? language)
    {
        Log.LogDebug(logger, "filter passed to search {filters}", query.Serialize());
        var request = new CatalogFilterRequest()
        {
            Query = query,
            External = this.IsExternalSource(source),
            Language = language
        };

        return this.QueryHandler(request, (Abstractions.Models.SwaaSes.SwaasCatalog o) => o.MapToResponse(this.HttpContext.Request)!);
    }
    #endregion

    #region Virtual switches

    /// <summary>
    /// Search virtual switches
    /// </summary>
    [HttpGet("{id}/virtualSwitches")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<VirtualSwitchListResponseDto> SearchVirtualSwitch(string? id, [FromCeSource] string? source, string projectId, [FromQuery] ResourceQueryDefinition query)
    {
        Log.LogDebug(logger, "filter passed to search {filters}", query.Serialize());
        var request = new VirtualSwitchSearchFilterRequest()
        {
            Query = query,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            External = this.IsExternalSource(source),
            SwaasId = id,
        };

        return this.QueryHandler<VirtualSwitchSearchFilterRequest, VirtualSwitchList>(request, o => o.MapToResponse(this.HttpContext.Request)!);
    }

    /// <summary>
    /// Get virtual switch by id
    /// </summary>
    [HttpGet("{id}/virtualSwitches/{virtualSwitchId}")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<VirtualSwitchResponseDto> GetVirtualSwitchById(string? id, string? virtualSwitchId, [FromCeSource] string? source, string projectId)
    {
        var request = new VirtualSwitchGetByIdRequest()
        {
            SwaasId = id,
            VirtualSwitchId = virtualSwitchId,
            UserId = User.GetUserId(),
            ProjectId = projectId
        };

        return this.QueryHandler<VirtualSwitchGetByIdRequest, VirtualSwitch>(request, m => m.MapToResponse()!);
    }

    /// <summary>
    /// Get services linkable to the given virtual switch 
    /// </summary>
    [HttpGet("{id}/virtualSwitches/{virtualSwitchId}/linkableservices")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<List<LinkableServiceResponseDto>> GetLinkableServices(string? id, string? virtualSwitchId, [FromCeSource] string? source, string projectId, [FromQuery] ResourceQueryDefinition query)
    {
        var request = new VirtualSwitchGetLinkableServicesRequest()
        {
            SwaasId = id,
            VirtualSwitchId = virtualSwitchId,
            UserId = User.GetUserId(),
            ProjectId = projectId
        };

        return this.QueryHandler<VirtualSwitchGetLinkableServicesRequest, List<LinkableService>>(request, m => m.MapToResponse());
    }

    /// <summary>
    /// Add virtual switch
    /// </summary>
    [HttpPost("{id}/virtualSwitches")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<VirtualSwitchResponseDto> AddVirtualSwitch([FromCeSource] string? source, string projectId, string id, [FromBody] VirtualSwitchAddDto model)
    {
        var request = new VirtualSwitchAddUseCaseRequest()
        {
            ResourceId = id,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            MessageBusRequestId = Guid.NewGuid().ToString(),
            VirtualSwitch = model
        };

        return this.UseCaseVirtualSwitches<VirtualSwitchAddUseCaseRequest, VirtualSwitchAddUseCaseResponse>(request);
    }

    /// <summary>
    /// Edit virtual switch
    /// </summary>
    [HttpPut("{id}/virtualSwitches/{virtualSwitchId}")]
    [AllowAnonymous]
    [Produce200Family(HttpStatusCode.OK)]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<VirtualSwitchResponseDto> EditVirtualSwitch([FromCeSource] string? source, string projectId, string id, string virtualSwitchId, [FromBody] VirtualSwitchEditDto model)
    {
        var request = new VirtualSwitchEditUseCaseRequest()
        {
            ResourceId = id,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            MessageBusRequestId = Guid.NewGuid().ToString(),
            Id = virtualSwitchId,
            Name = model?.Name
        };

        return this.UseCaseVirtualSwitches<VirtualSwitchEditUseCaseRequest, VirtualSwitchEditUseCaseResponse>(request);
    }

    /// <summary>
    /// Remove virtual switch
    /// </summary>
    [HttpDelete("{id}/virtualSwitches/{virtualSwitchId}")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult DeleteVirtualSwitch([FromCeSource] string? source, string projectId, string id, string virtualSwitchId)
    {
        var request = new VirtualSwitchDeleteUseCaseRequest()
        {
            ResourceId = id,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            MessageBusRequestId = Guid.NewGuid().ToString(),
            Id = virtualSwitchId
        };

        return this.UseCase<VirtualSwitchDeleteUseCaseRequest, VirtualSwitchDeleteUseCaseResponse>(request);
    }
    #endregion

    #region Virtual switch Links

    /// <summary>
    /// Search virtual switch lins
    /// </summary>
    [HttpGet("{id}/virtualswitchlinks")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<VirtualSwitchLinkListResponseDto> SearchVirtualSwitchLinks(string? id, [FromCeSource] string? source, string projectId, [FromQuery] ResourceQueryDefinition query)
    {
        Log.LogDebug(logger, "filter passed to search {filters}", query.Serialize());
        var request = new VirtualSwitchLinkSearchFilterRequest()
        {
            Query = query,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            External = this.IsExternalSource(source),
            SwaasId = id,
        };

        return this.QueryHandler<VirtualSwitchLinkSearchFilterRequest, VirtualSwitchLinkList>(request, o => o.MapToResponse(this.HttpContext.Request)!);
    }

    /// <summary>
    /// Get services linkable to the given virtual switch 
    /// </summary>
    [HttpGet("{id}/virtualswitchlinks/{virtualSwitchLinkId}")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<VirtualSwitchResponseDto> GetVirtualSwitchLinkById(string? id, string? virtualSwitchLinkId, [FromCeSource] string? source, string projectId)
    {
        var request = new VirtualSwitchLinkGetByIdRequest()
        {
            SwaasId = id,
            VirtualSwitchLinkId = virtualSwitchLinkId,
            UserId = User.GetUserId(),
            ProjectId = projectId
        };

        return this.QueryHandler<VirtualSwitchLinkGetByIdRequest, VirtualSwitchLink>(request, m => m.MapToResponse()!);
    }

    /// <summary>
    /// Add virtual switch link
    /// </summary>
    [HttpPost("{id}/virtualswitchlinks")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<VirtualSwitchResponseDto> AddVirtualSwitchLink([FromCeSource] string? source, string projectId, string id, [FromBody] VirtualSwitchLinkAddDto model)
    {
        var request = new VirtualSwitchLinkAddUseCaseRequest()
        {
            ResourceId = id,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            MessageBusRequestId = Guid.NewGuid().ToString(),
            Link = model
        };

        return this.UseCaseVirtualSwitchLinks<VirtualSwitchLinkAddUseCaseRequest, VirtualSwitchLinkAddUseCaseResponse>(request);
    }

    /// <summary>
    /// Remove virtual switch link
    /// </summary>
    [HttpDelete("{id}/virtualswitchlinks/{linkId}")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult DeleteVirtualSwitchLink([FromCeSource] string? source, string projectId, string id, string linkId)
    {
        var request = new VirtualSwitchLinkDeleteUseCaseRequest()
        {
            ResourceId = id,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            MessageBusRequestId = Guid.NewGuid().ToString(),
            Id = linkId
        };

        return this.UseCase<VirtualSwitchLinkDeleteUseCaseRequest, VirtualSwitchLinkDeleteUseCaseResponse>(request);
    }

    ///// <summary>
    ///// Add virtual switch
    ///// </summary>
    //[HttpPost("{id}/virtualSwitches")]
    //[AllowAnonymous]
    //[SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    //public ActionResult<VirtualSwitchResponseDto> AddVirtualSwitch([FromCeSource] string? source, string projectId, string id, [FromBody] VirtualSwitchAddDto model)
    //{
    //    var request = new VirtualSwitchAddUseCaseRequest()
    //    {
    //        ResourceId = id,
    //        UserId = User.GetUserId(),
    //        ProjectId = projectId,
    //        MessageBusRequestId = Guid.NewGuid().ToString(),
    //        VirtualSwitch = model
    //    };

    //    return this.UseCaseVirtualSwitches<VirtualSwitchAddUseCaseRequest, VirtualSwitchAddUseCaseResponse>(request);
    //}

    ///// <summary>
    ///// Edit virtual switch
    ///// </summary>
    //[HttpPut("{id}/virtualSwitches/{vsId}")]
    //[AllowAnonymous]
    //[Produce200Family(HttpStatusCode.OK)]
    //[SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    //public ActionResult<VirtualSwitchResponseDto> EditVirtualSwitch([FromCeSource] string? source, string projectId, string id, int vsId, [FromBody] VirtualSwitchEditDto model)
    //{
    //    var request = new VirtualSwitchEditUseCaseRequest()
    //    {
    //        ResourceId = id,
    //        UserId = User.GetUserId(),
    //        ProjectId = projectId,
    //        MessageBusRequestId = Guid.NewGuid().ToString(),
    //        Id = vsId,
    //        Name = model?.Name
    //    };

    //    return this.UseCaseVirtualSwitches<VirtualSwitchEditUseCaseRequest, VirtualSwitchEditUseCaseResponse>(request);
    //}

    ///// <summary>
    ///// Remove virtual switch
    ///// </summary>
    //[HttpDelete("{id}/virtualSwitches/{vsId}")]
    //[AllowAnonymous]
    //[SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    //public ActionResult<VirtualSwitchResponseDto> DeleteVirtualSwitch([FromCeSource] string? source, string projectId, string id, int vsId)
    //{
    //    var request = new VirtualSwitchDeleteUseCaseRequest()
    //    {
    //        ResourceId = id,
    //        UserId = User.GetUserId(),
    //        ProjectId = projectId,
    //        MessageBusRequestId = Guid.NewGuid().ToString(),
    //        Id = vsId
    //    };

    //    return this.UseCaseVirtualSwitches<VirtualSwitchDeleteUseCaseRequest, VirtualSwitchDeleteUseCaseResponse>(request);
    //}

    ///// <summary>
    ///// Add virtual switch link
    ///// </summary>
    //[HttpPost("{id}/virtualSwitches/{vsId}/links")]
    //[AllowAnonymous]
    //[SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    //public ActionResult<VirtualSwitchResponseDto> AddVirtualSwitchLink([FromCeSource] string? source, string projectId, string id, int vsId, [FromBody] VirtualSwitchLinkAddDto model)
    //{
    //    var request = new VirtualSwitchLinkAddUseCaseRequest()
    //    {
    //        ResourceId = id,
    //        UserId = User.GetUserId(),
    //        ProjectId = projectId,
    //        MessageBusRequestId = Guid.NewGuid().ToString(),
    //        VirtualSwitchLinkId = vsId,
    //        Link = model
    //    };

    //    return this.UseCaseVirtualSwitchLinks<VirtualSwitchLinkAddUseCaseRequest, VirtualSwitchLinkAddUseCaseResponse>(request);
    //}    

    #endregion
}
