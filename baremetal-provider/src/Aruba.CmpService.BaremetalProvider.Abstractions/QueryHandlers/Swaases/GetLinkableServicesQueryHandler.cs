using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SwaaSes;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Swaases.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Swaases;

public class GetLinkableServicesQueryHandler :
    IQueryHandler<VirtualSwitchGetLinkableServicesRequest, List<LinkableService>>
{
    private readonly ISwaasesService swaasesService;

    public GetLinkableServicesQueryHandler(ISwaasesService swaasesService)
    {
        this.swaasesService = swaasesService;
    }

    public async Task<List<LinkableService>?> Handle(VirtualSwitchGetLinkableServicesRequest request)
    {
        var ret = await this.swaasesService.GetLinkableServices(request, CancellationToken.None).ConfigureAwait(false);
        return ret.Value;
    }
}
