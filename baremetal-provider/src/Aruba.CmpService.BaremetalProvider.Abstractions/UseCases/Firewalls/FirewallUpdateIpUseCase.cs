using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Firewalls.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Firewalls.Responses;
using Aruba.MessageBus.Transactions;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Firewalls;

[NonTransactional]
public class FirewallUpdateIpUseCase : FirewallNoResponseBaseUseCase<FirewallUpdateIpUseCaseRequest, FirewallUpdateIpUseCaseResponse>
{
    public FirewallUpdateIpUseCase(IFirewallsService firewallsService) :
        base(firewallsService)
    {
    }

    protected override async Task<ServiceResult> ExecuteService(FirewallUpdateIpUseCaseRequest request, CancellationToken cancellationToken)
    {
        var ret = await this.FirewallsService.UpdateIpAddress(request, cancellationToken).ConfigureAwait(false);
        return ret;

    }
}
