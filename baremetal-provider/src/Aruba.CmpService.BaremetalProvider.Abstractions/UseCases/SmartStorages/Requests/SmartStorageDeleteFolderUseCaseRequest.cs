using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages.Requests;
public class SmartStorageDeleteFolderUseCaseRequest : BaseUserUseCaseRequest
{
    /// <summary>
    /// Snapshot Id
    /// </summary>
    public string? SmartFolderId { get; set; }
}

