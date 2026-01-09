using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages.Requests;
public class SmartStorageUpdateSnapshotTaskUseCaseRequest : BaseUserUseCaseRequest
{
    public int? SnapshotId { get; set; }
    public bool? Enable { get; set; }
}

