using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages.Responses;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages;
public class SmartStorageUpdateSnapshotTaskUseCase : SmartStorageNoResponseBaseUseCase<SmartStorageUpdateSnapshotTaskUseCaseRequest, SmartStorageUpdateSnapshotTaskUseCaseResponse>
{
    public SmartStorageUpdateSnapshotTaskUseCase(ISmartStoragesService smartStoragesService)
        : base(smartStoragesService)
    {

    }
    protected override async Task<ServiceResult> ExecuteService(SmartStorageUpdateSnapshotTaskUseCaseRequest request, CancellationToken cancellationToken)
    {
        var ret = await SmartStoragesService.UpdateSnapshotTask(request).ConfigureAwait(false);
        return ret;
    }
}
