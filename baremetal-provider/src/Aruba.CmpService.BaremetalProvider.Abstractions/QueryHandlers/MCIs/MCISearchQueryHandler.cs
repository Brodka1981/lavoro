using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.MCIs;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.MCIs.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers;

public class MCISearchQueryHandler : IQueryHandler<MCISearchFilterRequest, MCIList>
{
    private readonly IMCIsService mciService;

    public MCISearchQueryHandler(IMCIsService mciService)
    {
        this.mciService = mciService;
    }

    public async Task<MCIList> Handle(MCISearchFilterRequest request)
    {
        ParametersCheck(request);

        var mcis = await this.mciService.Search(request, CancellationToken.None).ConfigureAwait(false);
        return mcis.Value!;
    }

    private static void ParametersCheck(MCISearchFilterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
    }
}
