using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages.Responses;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages;

public class SmartStorageDisableProtocolUseCase : SmartStorageNoResponseBaseUseCase<SmartStorageDisableProtocolUseCaseRequest, SmartStorageDisableProtocolUseCaseResponse>
{
    public SmartStorageDisableProtocolUseCase(ISmartStoragesService smartStoragesService)
        : base(smartStoragesService)
    {

    }

    protected override async Task<ServiceResult> ExecuteService(SmartStorageDisableProtocolUseCaseRequest request, CancellationToken cancellationToken)
    {
        var ret = await SmartStoragesService.DisableProtocol(request).ConfigureAwait(false);
        return ret;
    }
}
