using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages.Requests;
public class SmartStorageApplySnapshotUseCaseRequest : BaseUserUseCaseRequest
{
    /// <summary>
    /// Snapshot Id
    /// </summary>
    public string? SnapshotId { get; set; }
}
