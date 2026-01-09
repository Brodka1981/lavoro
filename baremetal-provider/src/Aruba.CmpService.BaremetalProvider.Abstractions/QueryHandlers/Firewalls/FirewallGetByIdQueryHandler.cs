using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Firewalls;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Firewalls.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers;

public class FirewallGetByIdQueryHandler : IQueryHandler<FirewallByIdRequest, Firewall>
{
    private readonly IFirewallsService firewallService;

    public FirewallGetByIdQueryHandler(IFirewallsService firewallService)
    {
        this.firewallService = firewallService;
    }

    public async Task<Firewall?> Handle(FirewallByIdRequest request)
    {
        ParametersCheck(request);

        var result = await this.firewallService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        if (!result.Errors.Any())
        {
            return result.Value;
        }
        return null;
    }

    private static void ParametersCheck(FirewallByIdRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
    }
}
