using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Firewalls;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Firewalls.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Firewalls.Responses;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Servers;

public class FirewallSetAutomaticRenewUseCase : FirewallNoResponseBaseUseCase<FirewallSetAutomaticRenewUseCaseRequest, FirewallSetAutomaticRenewUseCaseResponse>
{
    public FirewallSetAutomaticRenewUseCase(IFirewallsService FirewallsService)
    : base(FirewallsService)
    {
    }

    protected override async Task<ServiceResult> ExecuteService(FirewallSetAutomaticRenewUseCaseRequest request, CancellationToken cancellationToken)
    {
        var ret = await FirewallsService.SetAutomaticRenew(request, cancellationToken).ConfigureAwait(false);
        return ret;
    }
}
