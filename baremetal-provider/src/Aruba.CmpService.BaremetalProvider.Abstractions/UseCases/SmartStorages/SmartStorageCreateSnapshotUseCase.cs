using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages.Responses;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages;
public class SmartStorageCreateSnapshotUseCase : SmartStorageNoResponseBaseUseCase<SmartStorageCreateSnapshotUseCaseRequest, SmartStorageCreateSnapshotUseCaseResponse>
{
    public SmartStorageCreateSnapshotUseCase(ISmartStoragesService smartStoragesService)
        : base(smartStoragesService)
    {

    }
    protected override async Task<ServiceResult> ExecuteService(SmartStorageCreateSnapshotUseCaseRequest request, CancellationToken cancellationToken)
    {
        var ret = await SmartStoragesService.CreateSnapshot(request).ConfigureAwait(false);
        return ret;
    }
}
