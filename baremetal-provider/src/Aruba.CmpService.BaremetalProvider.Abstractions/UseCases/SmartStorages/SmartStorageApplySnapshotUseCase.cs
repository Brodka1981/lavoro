using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages.Responses;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages;
public class SmartStorageApplySnapshotUseCase : SmartStorageNoResponseBaseUseCase<SmartStorageApplySnapshotUseCaseRequest, SmartStorageApplySnapshotUseCaseResponse>
{
    public SmartStorageApplySnapshotUseCase(ISmartStoragesService smartStoragesService)
        : base(smartStoragesService)
    {

    }
    protected override async Task<ServiceResult> ExecuteService(SmartStorageApplySnapshotUseCaseRequest request, CancellationToken cancellationToken)
    {
        var ret = await SmartStoragesService.ApplySnapshot(request).ConfigureAwait(false);
        return ret;
    }
}
