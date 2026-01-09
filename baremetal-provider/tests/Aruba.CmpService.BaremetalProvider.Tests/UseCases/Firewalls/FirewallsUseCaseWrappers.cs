using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Firewalls;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Firewalls.Requests;

namespace Aruba.CmpService.BaremetalProvider.Tests.UseCases.Firewalls;

public class FirewallRenameUseCaseWrapper :
    FirewallRenameUseCase
{
    public FirewallRenameUseCaseWrapper(IFirewallsService firewallsService) :
        base(firewallsService)
    {

    }
    public async Task<ServiceResult> Execute(FirewallRenameUseCaseRequest request)
    {
        return await base.ExecuteService(request, CancellationToken.None).ConfigureAwait(false);
    }
}

public class FirewallUpdateIpUseCaseWrapper :
    FirewallUpdateIpUseCase
{
    public FirewallUpdateIpUseCaseWrapper(IFirewallsService firewallsService) :
        base(firewallsService)
    {

    }
    public async Task<ServiceResult> Execute(FirewallUpdateIpUseCaseRequest request)
    {
        return await base.ExecuteService(request, CancellationToken.None).ConfigureAwait(false);
    }
}