using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Switches;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Switches.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Switches;

public class SwitchSearchQueryHandler : IQueryHandler<SwitchSearchFilterRequest, SwitchList>
{
    private readonly ISwitchesService switchesService;

    public SwitchSearchQueryHandler(ISwitchesService switchesService)
    {
        this.switchesService = switchesService;
    }

    public async Task<SwitchList> Handle(SwitchSearchFilterRequest request)
    {
        ParametersCheck(request);
        var switches = await switchesService.Search(request, CancellationToken.None).ConfigureAwait(false);
        return switches.Value!;

    }

    private static void ParametersCheck(SwitchSearchFilterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
    }
}
