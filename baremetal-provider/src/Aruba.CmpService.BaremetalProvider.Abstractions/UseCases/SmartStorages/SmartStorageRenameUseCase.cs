using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages.Responses;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages;

public class SmartStorageRenameUseCase : SmartStorageNoResponseBaseUseCase<SmartStorageRenameUseCaseRequest, SmartStorageRenameUseCaseResponse>
{
    public SmartStorageRenameUseCase(ISmartStoragesService smartStoragesService)
        : base(smartStoragesService)
    { }

    protected override async Task<ServiceResult> ExecuteService(SmartStorageRenameUseCaseRequest request, CancellationToken cancellationToken)
    {
        var ret = await SmartStoragesService.Rename(request, cancellationToken).ConfigureAwait(false);
        return ret;
    }
}
