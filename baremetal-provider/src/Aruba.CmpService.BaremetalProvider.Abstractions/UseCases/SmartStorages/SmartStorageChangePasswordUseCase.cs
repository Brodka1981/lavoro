using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages.Responses;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages;

public class SmartStorageChangePasswordUseCase : SmartStorageNoResponseBaseUseCase<SmartStorageChangePasswordUseCaseRequest, SmartStorageChangePasswordUseCaseResponse>
{
    public SmartStorageChangePasswordUseCase(ISmartStoragesService smartStoragesService)
        : base(smartStoragesService)
    {

    }
    protected async override Task<ServiceResult> ExecuteService(SmartStorageChangePasswordUseCaseRequest request, CancellationToken cancellationToken)
    {
        var ret = await SmartStoragesService.ChangePassword(request, cancellationToken).ConfigureAwait(false);
        return ret;
    }
}
