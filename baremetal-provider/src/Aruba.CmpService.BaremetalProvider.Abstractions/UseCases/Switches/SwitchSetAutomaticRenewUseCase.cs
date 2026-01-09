using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Switches.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Switches.Responses;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Switches;

public class SwitchSetAutomaticRenewUseCase : SwitchNoResponseBaseUseCase<SwitchSetAutomaticRenewUseCaseRequest, SwitchSetAutomaticRenewUseCaseResponse>
{
    public SwitchSetAutomaticRenewUseCase(ISwitchesService SwitchesService)
    : base(SwitchesService)
    {
    }

    protected override async Task<ServiceResult> ExecuteService(SwitchSetAutomaticRenewUseCaseRequest request, CancellationToken cancellationToken)
    {
        var ret = await SwitchesService.SetAutomaticRenew(request, cancellationToken).ConfigureAwait(false);
        return ret;
    }
}
