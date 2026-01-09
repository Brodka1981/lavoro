using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages.Responses;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages;

public class SmartStorageEnableProtocolUseCase : SmartStorageNoResponseBaseUseCase<SmartStorageEnableProtocolUseCaseRequest, SmartStorageEnableProtocolUseCaseResponse>
{
    public SmartStorageEnableProtocolUseCase(ISmartStoragesService smartStoragesService)
        : base(smartStoragesService)
    {

    }

    protected override async Task<ServiceResult> ExecuteService(SmartStorageEnableProtocolUseCaseRequest request, CancellationToken cancellationToken)
    {
        var ret = await SmartStoragesService.EnableProtocol(request).ConfigureAwait(false);
        return ret;
    }
}
