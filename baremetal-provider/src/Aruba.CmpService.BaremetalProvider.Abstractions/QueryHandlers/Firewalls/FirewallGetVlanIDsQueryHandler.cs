
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Firewalls;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Firewalls.Requests;
using Throw;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Firewalls;
public class FirewallGetVlanIDsQueryHandler : IQueryHandler<FirewallVlanIdFilterRequest, FirewallVlanIdList>
{
    private readonly IFirewallsService firewallsService;

    public FirewallGetVlanIDsQueryHandler(IFirewallsService firewallsService)
    {
        this.firewallsService = firewallsService;
    }

    public async Task<FirewallVlanIdList> Handle(FirewallVlanIdFilterRequest request)
    {
        request.ThrowIfNull();

        var vlanids = await this.firewallsService.GetVlanIds(request, CancellationToken.None).ConfigureAwait(false);

        if (!vlanids.Errors.Any())
        {
            return vlanids.Value;
        }
        return null;
    }
}
