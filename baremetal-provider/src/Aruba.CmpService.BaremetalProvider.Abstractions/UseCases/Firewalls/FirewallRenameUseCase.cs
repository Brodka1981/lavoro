using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Firewalls.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Firewalls.Responses;
using Aruba.MessageBus.Transactions;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Firewalls;

[NonTransactional]
public class FirewallRenameUseCase : FirewallNoResponseBaseUseCase<FirewallRenameUseCaseRequest, FirewallRenameUseCaseResponse>
{
    public FirewallRenameUseCase(IFirewallsService firewallsService)
        : base(firewallsService)
    { }

    protected override async Task<ServiceResult> ExecuteService(FirewallRenameUseCaseRequest request, CancellationToken cancellationToken)
    {
        var ret = await FirewallsService.Rename(request, cancellationToken).ConfigureAwait(false);
        return ret;
    }
}
