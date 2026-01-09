using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages.Requests;
public class SmartStorageCreateSnapshotUseCaseRequest : BaseUserUseCaseRequest
{
    /// <summary>
    /// The name of the folder
    /// </summary>
    public string? FolderName { get; set; }
}

