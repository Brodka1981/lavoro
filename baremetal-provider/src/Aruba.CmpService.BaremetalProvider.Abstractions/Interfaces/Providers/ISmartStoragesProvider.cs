using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.SmartStorages;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;

public interface ISmartStoragesProvider :
    ILegacyProvider<LegacySmartStorageListItem, LegacySmartStorageDetail>
{
    /// <summary>
    /// Get protocols by smart storage id
    /// </summary>
    /// <param name="id">Smart storage id</param>
    Task<ApiCallOutput<List<LegacyProtocol>>> GetProtolList(string id);

    /// <summary>
    /// Enable or disable smart storage protocol 
    /// </summary>
    /// <param name="id">smart storage id</param>
    /// <param name="serviceType">protocol type</param>
    /// <param name="enable">TRUE to enable, FALSE to disable the service</param>
    /// <returns>TRUE if success</returns>
    Task<ApiCallOutput<bool>> ToggleProtocol(string id, ServiceType serviceType, bool enable);

    /// <summary>
    /// Enable or disable smart storage protocol 
    /// </summary>
    /// <param name="id">smart storage id</param>
    /// <param name="password">smart storage password</param>
    /// <returns>TRUE if success</returns>
    Task<ApiCallOutput<bool>> ActivateSmartStorage(string id, string password);

    /// <summary>
    /// Get smart storage statistics
    /// </summary>
    /// <param name="id">smart storage id</param>
    /// <returns>Smart storage statistics</returns>
    Task<ApiCallOutput<LegacyStatistics>> GetStatistics(string id);

    /// <summary>
    /// Get smart storage snapshots
    /// </summary>
    /// <param name="id">smart storage id</param>
    /// <returns>Smart storage snapshots</returns>
    Task<ApiCallOutput<LegacySnapshots>> GetSnapshots(string id);

    /// <summary>
    /// Delete smart storage snapshot
    /// </summary>
    /// <param name="id">smart storage id</param>
    /// <param name="snapshotId">snapshot id</param>
    /// <returns>TRUE if success</returns>
    Task<ApiCallOutput<bool>> DeleteSnapshot(string id, string snapshotId);

    /// <summary>
    /// Delete smart storage snapshot task
    /// </summary>
    /// <param name="id">smart storage id</param>
    /// <param name="snapshotId">snapshot id</param>
    /// <returns>TRUE if success</returns>
    Task<ApiCallOutput<bool>> DeleteSnapshotTask(string id, Int32 snapshotId);


    /// <summary>
    /// create new Smart Storage folder
    /// </summary>
    /// <param name="id">smart storage id</param>
    /// <param name="name">folder name</param>
    Task<ApiCallOutput<bool>> CreateSmartFolder(string id, string name);

    /// <summary>
    /// Delete smart storage folder
    /// </summary>
    /// <param name="id">smart storage id</param>
    /// <param name="smartFolderId">smart folder id</param>
    /// <returns>TRUE if success</returns>
    Task<ApiCallOutput<bool>> DeleteSmartFolder(string id, string smartFolderId);

    /// <summary>
    /// Create snapshot task
    /// </summary>
    /// <param name="id">smart storage id</param>
    /// <param name="snapshot"></param>
    /// <param name="isRootFolder"></param>
    /// <returns></returns>
    Task<ApiCallOutput<bool>> CreateSnapshotTask(string id, SmartStorageCreateSnapshotTaskDto snapshot, bool isRootFolder = false);

    /// <summary>
    /// Create manual snapshot 
    /// </summary>
    /// <param name="id">smart storage id</param>
    /// <param name="smartFolderName">smart fodler name</param>
    /// <param name="isRootFolder">smart fodler name</param>
    /// <returns></returns>
    Task<ApiCallOutput<bool>> CreateSnapshot(string id, string smartFolderName, bool isRootFolder = false);


    /// <summary>
    /// Enable or disable  snapshot task
    /// </summary>
    /// <param name="id"></param>
    /// <param name="snapshotTaskId"></param>
    /// <param name="enable"></param>
    /// <returns></returns>
    Task<ApiCallOutput<bool>> UpdateSnapshotTask(string id, int snapshotTaskId, bool enable);

    /// <summary>
    /// Apply manual snapshot 
    /// </summary>
    /// <param name="id">smart storage id</param>
    /// <param name="snapshotId">snapshot id</param>
    /// <returns></returns>
    Task<ApiCallOutput<bool>> RestoreSnapshot(string id, string snapshotId);

    /// <summary>
    /// Get smart folders
    /// </summary>
    /// <param name="id">smart storage id</param>
    /// <returns></returns>
    Task<ApiCallOutput<LegacySmartFolders>> GetSmartFolders(long id);
}
