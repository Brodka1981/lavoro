using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Firewalls;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Firewalls.Requests;
using Throw;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers;

public class FirewallSearchIpAddressesQueryHandler :
    IQueryHandler<FirewallIpAddressesFilterRequest, FirewallIpAddressList>
{
    private readonly IFirewallsService firewallsService;

    public FirewallSearchIpAddressesQueryHandler(IFirewallsService firewallsService)
    {
        this.firewallsService = firewallsService;
    }

    public async Task<FirewallIpAddressList> Handle(FirewallIpAddressesFilterRequest request)
    {
        request.ThrowIfNull();

        var firewalls = await this.firewallsService.SearchIpAddresses(request, CancellationToken.None).ConfigureAwait(false);

        if (!firewalls.Errors.Any())
        {
            return firewalls.Value;
        }
        return null;
    }
}
