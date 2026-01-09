using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Switches;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Switches.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Switches;

public class SwitchGetByIdQueryHandler : IQueryHandler<SwitchByIdRequest, Switch>
{
    private readonly ISwitchesService switchesService;

    public SwitchGetByIdQueryHandler(ISwitchesService switchesService)
    {
        this.switchesService = switchesService;
    }

    public async Task<Switch?> Handle(SwitchByIdRequest request)
    {
        ParametersCheck(request);

        var result = await switchesService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        if (!result.Errors.Any())
        {
            return result.Value;
        }
        return null;
    }

    private static void ParametersCheck(SwitchByIdRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
    }
}
