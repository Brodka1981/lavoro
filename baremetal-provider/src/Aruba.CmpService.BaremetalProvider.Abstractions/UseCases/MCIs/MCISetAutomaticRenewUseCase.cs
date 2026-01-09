using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.MCIs.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.MCIs.Responses;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.MCIs;

public class MCISetAutomaticRenewUseCase : MCINoResponseBaseUseCase<MCISetAutomaticRenewUseCaseRequest, MCISetAutomaticRenewUseCaseResponse>
{
    public MCISetAutomaticRenewUseCase(IMCIsService mcisService)
        : base(mcisService)
    { }

    protected override async Task<ServiceResult> ExecuteService(MCISetAutomaticRenewUseCaseRequest request, CancellationToken cancellationToken)
    {
        var ret = await MCIsService.SetAutomaticRenew(request, cancellationToken).ConfigureAwait(false);
        return ret;
    }
}
