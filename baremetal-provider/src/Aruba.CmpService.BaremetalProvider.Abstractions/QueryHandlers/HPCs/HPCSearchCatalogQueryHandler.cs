using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.HPCs;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.HPCs;

public class HPCSearchCatalogQueryHandler : IQueryHandler<CatalogFilterRequest, HPCCatalog>
{
    private readonly IHPCsService hpcsService;

    public HPCSearchCatalogQueryHandler(IHPCsService hpcsService)
    {
        this.hpcsService = hpcsService;
    }

    public async Task<HPCCatalog> Handle(CatalogFilterRequest request)
    {
        ParametersCheck(request);

        var catalog = await hpcsService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
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
