using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages.Requests;
public class SmartStorageCreateSnapshotTaskUseCaseRequest : BaseUserUseCaseRequest
{
    /// <summary>
    /// The dto of snapshot task to create
    /// </summary>
    public SmartStorageCreateSnapshotTaskDto SnapshotTask { get; set; }
}

