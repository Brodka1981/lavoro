using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Switches;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Switches.Requests;

namespace Aruba.CmpService.BaremetalProvider.Tests.UseCases.Switches;

public class SwitchRenameUseCaseWrapper :
    SwitchRenameUseCase
{
    public SwitchRenameUseCaseWrapper(ISwitchesService SwitchesService) :
        base(SwitchesService)
    {

    }
    public async Task<ServiceResult> Execute(SwitchRenameUseCaseRequest request)
    {
        return await base.ExecuteService(request, CancellationToken.None).ConfigureAwait(false);
    }

}
public class SwitchSetAutomaticRenewUseCaseWrapper : SwitchSetAutomaticRenewUseCase
{
    public SwitchSetAutomaticRenewUseCaseWrapper(ISwitchesService SwitchesService) :
        base(SwitchesService)
    {

    }
    public async Task<ServiceResult> Execute(SwitchSetAutomaticRenewUseCaseRequest request)
    {
        return await base.ExecuteService(request, CancellationToken.None).ConfigureAwait(false);
    }
}