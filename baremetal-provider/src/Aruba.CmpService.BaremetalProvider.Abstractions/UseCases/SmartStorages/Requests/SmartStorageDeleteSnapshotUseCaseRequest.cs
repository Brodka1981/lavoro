using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages.Requests;
public class SmartStorageDeleteSnapshotUseCaseRequest : BaseUserUseCaseRequest
{
    /// <summary>
    /// Snapshot Id
    /// </summary>
    public string? SnapshotId { get; set; }
}
