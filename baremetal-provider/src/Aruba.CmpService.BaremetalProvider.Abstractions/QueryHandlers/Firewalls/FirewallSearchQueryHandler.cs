using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Firewalls;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Firewalls.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers;

public class FirewallSearchQueryHandler : IQueryHandler<FirewallSearchFilterRequest, FirewallList>
{
    private readonly IFirewallsService firewallService;

    public FirewallSearchQueryHandler(IFirewallsService firewallService)
    {
        this.firewallService = firewallService;
    }

    public async Task<FirewallList> Handle(FirewallSearchFilterRequest request)
    {
        ParametersCheck(request);

        var firewalls = await this.firewallService.Search(request, CancellationToken.None).ConfigureAwait(false);
        return firewalls.Value!;
    }

    private static void ParametersCheck(FirewallSearchFilterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
    }
}
