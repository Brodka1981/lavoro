using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.MCIs;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.MCIs;

public class MCISearchCatalogQueryHandler : IQueryHandler<CatalogFilterRequest, MCICatalog>
{
    private readonly IMCIsService mcisService;

    public MCISearchCatalogQueryHandler(IMCIsService mcisService)
    {
        this.mcisService = mcisService;
    }

    public async Task<MCICatalog> Handle(CatalogFilterRequest request)
    {
        ParametersCheck(request);

        var catalog = await mcisService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
        if (!catalog.Errors.Any())
        {
            return catalog.Value!;
        }
        return null!;
    }

    private static void ParametersCheck(CatalogFilterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
    }
}
