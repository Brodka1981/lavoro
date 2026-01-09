using System.Net;
using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos;
using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.SmartStorages.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.SmartStorages.Responses;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages.Responses;
using Aruba.CmpService.BaremetalProvider.Abstractions.Utils;
using Aruba.CmpService.BaremetalProvider.Api.Code.ActionResults;
using Aruba.CmpService.BaremetalProvider.Api.Code.Controllers;
using Aruba.CmpService.BaremetalProvider.Api.Code.Filters;
using Aruba.CmpService.BaremetalProvider.Api.Code.Headers;
using Aruba.CmpService.BaremetalProvider.Api.Code.Swagger;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Aruba.CmpService.BaremetalProvider.Api.v1.Controllers;

[Route("projects/{projectId}/providers/Aruba.Baremetal/smartstorages")]
public class SmartStoragesController : BaseController
{
    private readonly ILogger<SmartStoragesController> logger;
    private readonly BaremetalOptions baremetalOptions;

    public SmartStoragesController(ILogger<SmartStoragesController> logger, IOptions<BaremetalOptions> baremetalOptions)
    {
        this.logger = logger;
        this.baremetalOptions = baremetalOptions.Value;
    }

    #region Smart Storage

    /// <summary>
    /// List smart storage
    /// </summary>
    [HttpGet()]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<SmartStorageListResponseDto> Search([FromCeSource] string? source, string projectId, [FromQuery] ResourceQueryDefinition query)
    {
        Log.LogDebug(logger, "filter passed to search {filters}", query.Serialize());
        var request = new SmartStorageSearchFilterRequest()
        {
            Query = query,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            External = this.IsExternalSource(source)
        };

        return this.QueryHandler<SmartStorageSearchFilterRequest, SmartStorageList>(request, o => o.MapToResponse(this.HttpContext.Request)!);
    }

    /// <summary>
    /// Get smart storage
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<SmartStorageResponseDto> Get([FromCeSource] string? source, string projectId, string id)
    {
        var request = new SmartStorageByIdRequest()
        {
            ResourceId = id,
            ProjectId = projectId,
            UserId = User.GetUserId(),
        };

        return this.QueryHandler<SmartStorageByIdRequest, SmartStorage>(request, m => m.MapToResponse());
    }

    /// <summary>
    /// Rename smart storage
    /// </summary>
    [HttpPut("{id}/name")]
    [AllowAnonymous]
    [Produce200Family(HttpStatusCode.OK)]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult Rename([FromCeSource] string? source, string projectId, string id, [FromBody] RenameDto model)
    {
        var request = new SmartStorageRenameUseCaseRequest()
        {
            ResourceId = id,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            MessageBusRequestId = Guid.NewGuid().ToString(),
            RenameData = model
        };

        return this.UseCase<SmartStorageRenameUseCaseRequest, SmartStorageRenameUseCaseResponse>(request);
    }

    /// <summary>
    /// Enable smart storage
    /// </summary>
    [HttpPost("{id}/activate")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    [Produce200Family(HttpStatusCode.OK)]
    public ActionResult Activate([FromCeSource] string? source, string projectId, string id, SmartStorageActivateDto model)
    {
        var request = new SmartStorageActivateUseCaseRequest()
        {
            ResourceId = id,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            MessageBusRequestId = Guid.NewGuid().ToString(),
            ActivateData = model
        };

        return this.UseCase<SmartStorageActivateUseCaseRequest, SmartStorageActivateUseCaseResponse>(request);
    }

    /// <summary>
    /// Change password
    /// </summary>
    [HttpPost("{id}/changepassword")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    [Produce200Family(HttpStatusCode.OK)]
    public ActionResult ChangePassword([FromCeSource] string? source, string projectId, string id, SmartStorageChangePasswordDto model)
    {
        var request = new SmartStorageChangePasswordUseCaseRequest()
        {
            ResourceId = id,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            MessageBusRequestId = Guid.NewGuid().ToString(),
            ActivateData = model
        };

        return this.UseCase<SmartStorageChangePasswordUseCaseRequest, SmartStorageChangePasswordUseCaseResponse>(request);
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
        var request = new SmartStorageSetAutomaticRenewUseCaseRequest()
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

        return this.UseCase<SmartStorageSetAutomaticRenewUseCaseRequest, SmartStorageSetAutomaticRenewUseCaseResponse>(request);
    }
    #endregion

    #region Protocols
    /// <summary>
    /// List protocols
    /// </summary>
    [HttpGet("{id}/protocols")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<IEnumerable<SmartStorageProtocol>> GetProtocols([FromCeSource] string? source, string projectId, string id)
    {
        var request = new SmartStorageProtocolsRequest()
        {
            ResourceId = id,
            UserId = User.GetUserId(),
            ProjectId = projectId,
        };

        return this.QueryHandler<SmartStorageProtocolsRequest, IEnumerable<SmartStorageProtocol>>(request);
    }

    /// <summary>
    /// Enable protocol
    /// </summary>
    [HttpPost("{id}/protocols")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    [Produce200Family(HttpStatusCode.OK)]
    public ActionResult EnableProtocol([FromCeSource] string? source, string projectId, string id, [FromBody] SmartStorageEnableProtocolDto enableProtocolDto)
    {
        var request = new SmartStorageEnableProtocolUseCaseRequest()
        {
            ResourceId = id,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            MessageBusRequestId = Guid.NewGuid().ToString(),
            ServiceType = enableProtocolDto.ServiceType
        };

        return this.UseCase<SmartStorageEnableProtocolUseCaseRequest, SmartStorageEnableProtocolUseCaseResponse>(request);
    }

    /// <summary>
    /// Disable protocol
    /// </summary>
    [HttpDelete("{id}/protocols/{serviceType}")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    [Produce200Family(HttpStatusCode.OK)]
    public ActionResult DisableProtocol([FromCeSource] string? source, string projectId, string id, string serviceType)
    {
        var request = new SmartStorageDisableProtocolUseCaseRequest()
        {
            ResourceId = id,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            MessageBusRequestId = Guid.NewGuid().ToString(),
        };

        if (!string.IsNullOrEmpty(serviceType)
            && System.Enum.TryParse(typeof(ServiceType), serviceType, ignoreCase: true, out var enumValue))
        {
            request.ServiceType = (ServiceType)enumValue;
        }

        return this.UseCase<SmartStorageDisableProtocolUseCaseRequest, SmartStorageDisableProtocolUseCaseResponse>(request);
    }
    #endregion

    #region Smart Folders
    /// <summary>
    /// List smart folders
    /// </summary>
    [HttpGet("{id}/smartfolders")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<IEnumerable<SmartStorageFoldersItemResponseDto>> GetSmartFolders([FromCeSource] string? source, string projectId, string id)
    {
        var request = new SmartStorageFoldersRequest()
        {
            ResourceId = id,
            UserId = User.GetUserId(),
            ProjectId = projectId,
        };

        return this.QueryHandler<SmartStorageFoldersRequest, IEnumerable<SmartStorageFoldersItem>>(request, o => o.MapToResponse(this.HttpContext.Request)!);
    }

    /// <summary>
    /// Get available smart folders
    /// </summary>
    [HttpGet("{id}/availablesmartfolders")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    [Produce200Family(HttpStatusCode.OK)]
    public ActionResult<GetAvailableSmartFoldersResponse> GetAvailableSmartFolders([FromCeSource] string? source, string projectId, string id)
    {
        var request = new GetAvailableSmartFoldersRequest()
        {
            ResourceId = id,
            UserId = User.GetUserId(),
            ProjectId = projectId,
        };

        return this.QueryHandler<GetAvailableSmartFoldersRequest, GetAvailableSmartFoldersResponse>(request, o => o);
    }

    /// <summary>
    /// Create smart folder
    /// </summary>
    [HttpPost("{id}/smartfolders")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    [Produce200Family(HttpStatusCode.OK)]
    public ActionResult CreateFolder([FromCeSource] string? source, string projectId, string id, SmartStorageCreateFolderDto smartStorageCreateFolderDto)
    {
        var request = new SmartStorageCreateFolderUseCaseRequest()
        {
            ResourceId = id,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            MessageBusRequestId = Guid.NewGuid().ToString(),
            Name = smartStorageCreateFolderDto.Name
        };

        return this.UseCase<SmartStorageCreateFolderUseCaseRequest, SmartStorageCreateFolderUseCaseResponse>(request);
    }

    /// <summary>
    /// Delete smart folder
    /// </summary>
    [HttpDelete("{id}/smartfolders/{smartFolderId}")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    [Produce200Family(HttpStatusCode.OK)]
    public ActionResult DeleteSmartFolder([FromCeSource] string? source, string projectId, string id, string smartFolderId)
    {
        var request = new SmartStorageDeleteFolderUseCaseRequest()
        {
            ResourceId = id,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            MessageBusRequestId = Guid.NewGuid().ToString(),
            SmartFolderId = smartFolderId
        };
        return this.UseCase<SmartStorageDeleteFolderUseCaseRequest, SmartStorageDeleteFolderUseCaseResponse>(request);
    }
    #endregion

    #region Statistics
    /// <summary>
    /// Get statistics
    /// </summary>
    [HttpGet("{id}/statistics")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<SmartStorageStatistics> GetStatistics([FromCeSource] string? source, string projectId, string id)
    {
        var request = new SmartStorageStatisticsRequest()
        {
            ResourceId = id,
            UserId = User.GetUserId(),
            ProjectId = projectId,
        };

        return this.QueryHandler<SmartStorageStatisticsRequest, SmartStorageStatistics>(request);
    }
    #endregion

    #region Snapshots
    /// <summary>
    /// List snapshots
    /// </summary>
    [HttpGet("{id}/snapshots")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<SmartStorageSnapshotsResponseDto> GetSnapshots([FromCeSource] string? source, string projectId, string id)
    {
        var request = new SmartStorageSnapshotsRequest()
        {
            ResourceId = id,
            UserId = User.GetUserId(),
            ProjectId = projectId,
        };

        return this.QueryHandler<SmartStorageSnapshotsRequest, SmartStorageSnapshots>(request, m => m.MapToResponse());
    }

    /// <summary>
    /// Create snapshot
    /// </summary>
    [HttpPost("{id}/snapshots/manuals")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<SmartStorageSnapshotsResponseDto> CreateSnapshot([FromCeSource] string? source, string projectId, string id, SmartStorageCreateSnapshotDto snapshot)
    {
        var request = new SmartStorageCreateSnapshotUseCaseRequest()
        {
            ResourceId = id,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            MessageBusRequestId = Guid.NewGuid().ToString(),
            FolderName = snapshot?.FolderName
        };
        return this.UseCase<SmartStorageCreateSnapshotUseCaseRequest, SmartStorageCreateSnapshotUseCaseResponse>(request);
    }

    /// <summary>
    /// Apply snapshot
    /// </summary>
    [HttpPost("{id}/snapshots/manuals/{snapshotId}/apply")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    public ActionResult<SmartStorageSnapshotsResponseDto> ApplySnapshot([FromCeSource] string? source, string projectId, string id, string snapshotId)
    {
        var request = new SmartStorageApplySnapshotUseCaseRequest()
        {
            ResourceId = id,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            MessageBusRequestId = Guid.NewGuid().ToString(),
            SnapshotId = WebUtility.UrlDecode(snapshotId)
        };
        return this.UseCase<SmartStorageApplySnapshotUseCaseRequest, SmartStorageApplySnapshotUseCaseResponse>(request);
    }

    /// <summary>
    /// Delete snapshot
    /// </summary>
    [HttpDelete("{id}/snapshots/manuals/{snapshotId}")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    [Produce200Family(HttpStatusCode.OK)]
    public ActionResult DeleteSnapshot([FromCeSource] string? source, string projectId, string id, string snapshotId)
    {
        var request = new SmartStorageDeleteSnapshotUseCaseRequest()
        {
            ResourceId = id,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            MessageBusRequestId = Guid.NewGuid().ToString(),
            SnapshotId = WebUtility.UrlDecode(snapshotId)
        };
        return this.UseCase<SmartStorageDeleteSnapshotUseCaseRequest, SmartStorageDeleteSnapshotUseCaseResponse>(request);
    }

    /// <summary>
    /// Create snapshot task
    /// </summary>
    [HttpPost("{id}/snapshots/tasks")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    [Produce200Family(HttpStatusCode.OK)]
    public ActionResult CreateSnapshotTask([FromCeSource] string? source, string projectId, string id, SmartStorageCreateSnapshotTaskDto snapshotTask)
    {
        var request = new SmartStorageCreateSnapshotTaskUseCaseRequest()
        {
            ResourceId = id,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            MessageBusRequestId = Guid.NewGuid().ToString(),
            SnapshotTask = snapshotTask
        };
        return this.UseCase<SmartStorageCreateSnapshotTaskUseCaseRequest, SmartStorageCreateSnapshotTaskUseCaseResponse>(request);
    }

    /// <summary>
    /// Update snapshot task status
    /// </summary>
    [HttpPut("{id}/snapshots/tasks/{snapshotId}")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    [Produce200Family(HttpStatusCode.OK)]
    public ActionResult UpdateSnapshotTask([FromCeSource] string? source, string projectId, string id, int snapshotId, SmartStorageUpdateSnapshotTaskDto snapshotTask)
    {
        var request = new SmartStorageUpdateSnapshotTaskUseCaseRequest()
        {
            ResourceId = id,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            MessageBusRequestId = Guid.NewGuid().ToString(),
            Enable = snapshotTask?.Enable,
            SnapshotId = snapshotId,
        };
        return this.UseCase<SmartStorageUpdateSnapshotTaskUseCaseRequest, SmartStorageUpdateSnapshotTaskUseCaseResponse>(request);
    }

    /// <summary>
    /// Delete snapshot task
    /// </summary>
    [HttpDelete("{id}/snapshots/tasks/{snapshotId}")]
    [AllowAnonymous]
    [SwaggerHeader(SwaggerHeaderAttribute.APIDOC)]
    [Produce200Family(HttpStatusCode.OK)]
    public ActionResult DeleteSnapshotTask([FromCeSource] string? source, string projectId, string id, int snapshotId)
    {
        var request = new SmartStorageDeleteSnapshotTaskUseCaseRequest()
        {
            ResourceId = id,
            UserId = User.GetUserId(),
            ProjectId = projectId,
            MessageBusRequestId = Guid.NewGuid().ToString(),
            SnapshotId = snapshotId
        };
        return this.UseCase<SmartStorageDeleteSnapshotTaskUseCaseRequest, SmartStorageDeleteSnapshotTaskUseCaseResponse>(request);
    }
    #endregion

    #region NoProjects

    /// <summary>
    /// Smart storages catalog
    /// </summary>
    [HttpGet("~/providers/Aruba.Baremetal/smartStorages/catalog")]
    [AllowAnonymous]
    public ActionResult<SmartStorageCatalogResponseDto> SearchCatalog([FromCeSource] string? source, [FromQuery] ResourceQueryDefinition query)
    {
        Log.LogDebug(logger, "filter passed to search {filters}", query.Serialize());
        var request = new CatalogFilterRequest()
        {
            Query = query,
            External = this.IsExternalSource(source)
        };

        return this.QueryHandler<CatalogFilterRequest, SmartStorageCatalog>(request, o => o.MapToResponse(this.HttpContext.Request)!);
    }

    /// <summary>
    /// Smart Storages guide
    /// </summary>
    [HttpGet("~/providers/Aruba.Baremetal/smartStorages/metadata")]
    [AllowAnonymous]
    public ActionResult<SmartStorageMetadataResponseDto> GetMetadata([FromCeSource] string? source)
    {
        return new SmartStorageMetadataResponseDto()
        {
            SmartStorageAiGuides = this.baremetalOptions.SmartStorageAiGuides
        };
    }
    #endregion
}
