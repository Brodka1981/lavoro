using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages.Responses;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages;
public class SmartStorageCreateSnapshotTaskUseCase : SmartStorageNoResponseBaseUseCase<SmartStorageCreateSnapshotTaskUseCaseRequest, SmartStorageCreateSnapshotTaskUseCaseResponse>
{
    public SmartStorageCreateSnapshotTaskUseCase(ISmartStoragesService smartStoragesService)
        : base(smartStoragesService)
    {

    }
    protected override async Task<ServiceResult> ExecuteService(SmartStorageCreateSnapshotTaskUseCaseRequest request, CancellationToken cancellationToken)
    {
        var ret = await SmartStoragesService.CreateSnapshotTask(request).ConfigureAwait(false);
        return ret;
    }
}
