using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SwaaSes;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Swaases.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Swaases;

public class VirtualSwitchLinkGetByIdQueryHandler : IQueryHandler<VirtualSwitchLinkGetByIdRequest, VirtualSwitchLink>
{
    private readonly ISwaasesService swaasesService;

    public VirtualSwitchLinkGetByIdQueryHandler(ISwaasesService swaasesService)
    {
        this.swaasesService = swaasesService;
    }

    public async Task<VirtualSwitchLink> Handle(VirtualSwitchLinkGetByIdRequest request)
    {
        ParametersCheck(request);

        var virtualSwitch = await swaasesService.GetVirtualSwitchLink(request).ConfigureAwait(false);
        return virtualSwitch.Value!;
    }

    private static void ParametersCheck(VirtualSwitchLinkGetByIdRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
    }
}
