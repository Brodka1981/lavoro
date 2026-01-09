using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages.Requests;
public class SmartStorageDeleteSnapshotTaskUseCaseRequest : BaseUserUseCaseRequest
{
    /// <summary>
    /// Snapshot Id
    /// </summary>
    public int SnapshotId { get; set; }
}

