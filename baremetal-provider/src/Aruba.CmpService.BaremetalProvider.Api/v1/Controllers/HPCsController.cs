using System.Net;
using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos;
using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Hpcs;
using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Servers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.HPCs;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.HPCs.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.HPCs.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.HPCs.Responses;
using Aruba.CmpService.BaremetalProvider.Abstractions.Utils;
using Aruba.CmpService.BaremetalProvider.Api.Code.ActionResults;
using Aruba.CmpService.BaremetalProvider.Api.Code.Controllers;
using Aruba.CmpService.BaremetalProvider.Api.Code.Filters;
using Aruba.CmpService.BaremetalProvider.Api.Code.Headers;
using Aruba.CmpService.BaremetalProvider.Api.Code.Swagger;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery;
using Microsoft.AspNetCore.Mvc;

namespace Aruba.CmpService.BaremetalProvider.Api.v1.Controllers;

[Route("projects/{projectId}/providers/Aruba.Baremetal/hpcs")]
public partial class HPCsController : BaseController
{
    private readonly ILogger<HPCsController> logger;

    public HPCsController(ILogger<HPCsController> logger)
    {
        this.logger = logger;
    }

    #region WithProjects
    /// <summary>
    /// Create HPC
    /// </summary>
    [HttpPost()]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<HPCResponseDto> Create([FromCeSource] string? source, string projectId, [FromBody] HPCResponseDto model)
    {
        var request = new HPCCreateUseCaseRequest()
        {
            MessageBusRequestId = Guid.NewGuid().ToString(),
            ProjectId = projectId,
            UserId = User.GetUserId(),
            // TODO: @martellata-hpc add properties here
        };
        return this.UseCase<HPCCreateUseCaseRequest, HPCCreateUseCaseResponse, HPC>(request, data =>
        {
            return new HPCResponseDto()
            {
                // TODO: @martellata-hpc map properties here
            };
        });
    }

    /// <summary>
    /// Get hpcs matching filters
    /// </summary>
    [HttpGet()]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<HPCListResponseDto> Search([FromCeSource] string? source, string projectId, [FromQuery] ResourceQueryDefinition query)
    {
        Log.LogDebug(logger, "filter passed to search {filters}", query.Serialize());
        var request = new HPCSearchFilterRequest()
        {
            Query = query,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            External = this.IsExternalSource(source)
        };

        return this.QueryHandler<HPCSearchFilterRequest, HPCList>(request, o => o.MapToResponse(this.HttpContext.Request)!);
    }

    /// <summary>
    /// Get an existing hpc by Id
    /// </summary>
    [HttpGet("{id}")]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<HPCResponseDto> Get([FromCeSource] string? source, string projectId, string id, [FromQuery] bool? calculatePrices = false)
    {
        var request = new HPCByIdRequest()
        {
            ResourceId = id,
            ProjectId = projectId,
            UserId = User.GetUserId(),
            CalculatePrices = calculatePrices ?? false
        };

        return this.QueryHandler<HPCByIdRequest, HPC>(request, m => m.MapToResponse());
    }

    /// <summary>
    /// Get an existing hpc by Id
    /// </summary>
    [HttpGet("{id}/services")]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<HPCListContentResponseDto> GetServices([FromCeSource] string? source, string projectId, string id, [FromQuery] ResourceQueryDefinition query)
    {
        var request = new HPCContentByIdRequest()
        {
            ResourceId = id,
            ProjectId = projectId,
            UserId = User.GetUserId(),
            Query = query
        };

        return this.QueryHandler<HPCContentByIdRequest, HPC>(request, m => m.MapToListResponse());
    }

    /// <summary>
    /// Rename hpc
    /// </summary>
    [HttpPut("{id}/name")]
    [Produce200Family(HttpStatusCode.OK)]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult Rename([FromCeSource] string? source, string projectId, string id, [FromBody] RenameDto model)
    {
        var request = new HPCRenameUseCaseRequest()
        {
            ResourceId = id,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            MessageBusRequestId = Guid.NewGuid().ToString(),
            RenameData = model
        };

        return this.UseCase<HPCRenameUseCaseRequest, HPCRenameUseCaseResponse>(request);
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
        var request = new HPCSetAutomaticRenewUseCaseRequest()
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
        return this.UseCase<HPCSetAutomaticRenewUseCaseRequest, HPCSetAutomaticRenewUseCaseResponse>(request);
        //return MockActionOK();
    }

    #endregion

    #region NoProjects
    /// <summary>
    /// MCI catalog
    /// </summary>
    [HttpGet("~/providers/Aruba.Baremetal/hpcs/catalog")]
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

        return this.QueryHandler(request, (Abstractions.Models.HPCs.HPCCatalog o) => o.MapToResponse()!);
    }

    #endregion

    #region MOCKS

    private ActionResult MockActionOK()
    {
        return Ok();
    }

    #endregion
}
