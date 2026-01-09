using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages.Responses;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages;

public class SmartStorageSetAutomaticRenewUseCase : SmartStorageNoResponseBaseUseCase<SmartStorageSetAutomaticRenewUseCaseRequest, SmartStorageSetAutomaticRenewUseCaseResponse>
{
    public SmartStorageSetAutomaticRenewUseCase(ISmartStoragesService SmartStoragesService)
    : base(SmartStoragesService)
    {
    }

    protected override async Task<ServiceResult> ExecuteService(SmartStorageSetAutomaticRenewUseCaseRequest request, CancellationToken cancellationToken)
    {
        var ret = await SmartStoragesService.SetAutomaticRenew(request, cancellationToken).ConfigureAwait(false);
        return ret;
    }
}
