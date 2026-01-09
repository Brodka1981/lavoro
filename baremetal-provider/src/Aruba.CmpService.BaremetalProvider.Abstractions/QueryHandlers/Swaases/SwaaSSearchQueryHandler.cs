using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SwaaSes;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Swaases.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Swaases;

public class SwaasSearchQueryHandler : IQueryHandler<SwaasSearchFilterRequest, SwaasList>
{
    private readonly ISwaasesService swaasesService;

    public SwaasSearchQueryHandler(ISwaasesService swaasesService)
    {
        this.swaasesService = swaasesService;
    }

    public async Task<SwaasList> Handle(SwaasSearchFilterRequest request)
    {
        ParametersCheck(request);


        var swaases = await swaasesService.Search(request, CancellationToken.None).ConfigureAwait(false);
        return swaases.Value!;

    }

    private static void ParametersCheck(SwaasSearchFilterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
    }
}
