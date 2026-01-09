using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.HPCs.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.HPCs.Responses;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.HPCs;

public class HPCSetAutomaticRenewUseCase : HPCNoResponseBaseUseCase<HPCSetAutomaticRenewUseCaseRequest, HPCSetAutomaticRenewUseCaseResponse>
{
    public HPCSetAutomaticRenewUseCase(IHPCsService mcisService)
        : base(mcisService)
    { }

    protected override async Task<ServiceResult> ExecuteService(HPCSetAutomaticRenewUseCaseRequest request, CancellationToken cancellationToken)
    {
        var ret = await HPCsService.SetAutomaticRenew(request, cancellationToken).ConfigureAwait(false);
        return ret;
    }
}
