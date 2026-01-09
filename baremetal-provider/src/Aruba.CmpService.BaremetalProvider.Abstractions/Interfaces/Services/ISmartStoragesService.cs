using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.SmartStorages.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.SmartStorages.Responses;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;

public interface ISmartStoragesService
{
    /// <summary>
    /// Search smart storages
    /// </summary>    
    Task<ServiceResult<SmartStorageList>> Search(SmartStorageSearchFilterRequest request, CancellationToken cancellationToken);
    
    /// <summary>
    /// Get smart storage by id
    /// </summary>
    Task<ServiceResult<SmartStorage>> GetById(BaseGetByIdRequest<SmartStorage> request, CancellationToken cancellationToken);

    /// <summary>
    /// Rename smart storage
    /// </summary>
    Task<ServiceResult> Rename(RenameUseCaseRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Search smart storage catalog
    /// </summary>
    Task<ServiceResult<SmartStorageCatalog>> SearchCatalog(CatalogFilterRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// get protocols
    /// </summary>
    Task<ServiceResult<IEnumerable<SmartStorageProtocol>>> GetProtocols(SmartStorageProtocolsRequest request);

    /// <summary>
    /// Enable protocol
    /// </summary>
    Task<ServiceResult> EnableProtocol(SmartStorageEnableProtocolUseCaseRequest request);

    /// <summary>
    /// Disable protocol
    /// </summary>
    Task<ServiceResult> DisableProtocol(SmartStorageDisableProtocolUseCaseRequest request);

    /// <summary>
    /// Search smart folders
    /// </summary>
    Task<ServiceResult<IEnumerable<SmartStorageFoldersItem>>> SearchFolders(SmartStorageFoldersRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Create smart folder
    /// </summary>
    Task<ServiceResult> CreateSmartFolder(SmartStorageCreateFolderUseCaseRequest request);

    /// <summary>
    /// Delete smart folder
    /// </summary>
    Task<ServiceResult> DeleteSmartFolder(SmartStorageDeleteFolderUseCaseRequest request);

    /// <summary>
    /// Create manual snapshot
    /// </summary>
    Task<ServiceResult> CreateSnapshot(SmartStorageCreateSnapshotUseCaseRequest request);

    /// <summary>
    /// Apply snapshot
    /// </summary>
    Task<ServiceResult> ApplySnapshot(SmartStorageApplySnapshotUseCaseRequest request);

    /// <summary>
    /// Delete manual snapshot
    /// </summary>
    Task<ServiceResult> DeleteSnapshot(SmartStorageDeleteSnapshotUseCaseRequest request);

    /// <summary>
    /// Create automatic snapshot
    /// </summary>
    Task<ServiceResult> CreateSnapshotTask(SmartStorageCreateSnapshotTaskUseCaseRequest request);

    /// <summary>
    /// Update automatic snapshot
    /// </summary>
    Task<ServiceResult> UpdateSnapshotTask(SmartStorageUpdateSnapshotTaskUseCaseRequest request);

    /// <summary>
    /// Delete automatic snapshot
    /// </summary>
    Task<ServiceResult> DeleteSnapshotTask(SmartStorageDeleteSnapshotTaskUseCaseRequest request);

    /// <summary>
    /// Activate smart storage
    /// </summary>
    Task<ServiceResult> Activate(SmartStorageActivateUseCaseRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Change password
    /// </summary>
    Task<ServiceResult> ChangePassword(SmartStorageChangePasswordUseCaseRequest request, CancellationToken cancellationToken);
    
    /// <summary>
    /// Get snapshots
    /// </summary>
    Task<ServiceResult<SmartStorageSnapshots>> GetSnapshots(SmartStorageSnapshotsRequest request);

    /// <summary>
    /// Get smart storage statistics
    /// </summary>
    Task<ServiceResult<SmartStorageStatistics>> GetStatistics(SmartStorageStatisticsRequest request);

    /// <summary>
    /// Get available smart folders count
    /// </summary>
    Task<ServiceResult<GetAvailableSmartFoldersResponse>> GetAvailableSmartFolders(GetAvailableSmartFoldersRequest request);

    /// <summary>
    /// Set automatic renew
    /// </summary>
    Task<ServiceResult> SetAutomaticRenew(SetAutomaticRenewUseCaseRequest request, CancellationToken cancellationToken);
}
