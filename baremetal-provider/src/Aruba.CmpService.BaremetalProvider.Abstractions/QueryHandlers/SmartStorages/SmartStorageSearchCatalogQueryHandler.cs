using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Firewalls;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.Sorting;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.SmartStorages;

public class SmartStorageSearchCatalogQueryHandler : IQueryHandler<CatalogFilterRequest, SmartStorageCatalog>
{
    private readonly ISmartStoragesService smartStoragesService;

    public SmartStorageSearchCatalogQueryHandler(ISmartStoragesService smartStoragesService)
    {
        this.smartStoragesService = smartStoragesService;
    }

    public async Task<SmartStorageCatalog> Handle(CatalogFilterRequest request)
    {
        ParametersCheck(request);

        var catalog = await smartStoragesService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
        if (!catalog.Errors.Any())
        {
            var catalogItems = catalog.Value!.Values;
            //Filtro per
            //ricerca fulltext 
            //sul campo Name
            var nameFilter = request.Query.Filters.FirstOrDefault(f => string.Equals(f.FieldName, "fulltextsearch", StringComparison.OrdinalIgnoreCase))?.Argument.As<string?>();
            if (!string.IsNullOrWhiteSpace(nameFilter))
            {
                catalogItems = catalogItems.Where(w => w.Name!.Contains(nameFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            long totalCount = catalogItems.Count;

            //Ordinamento
            //Campi da ordinare
            //Price,Name
            string? sortField = request.Query.Sorts.FirstOrDefault()?.FieldName?.ToUpperInvariant();
            bool sortDescending = request.Query.Sorts.FirstOrDefault()?.Direction == SortDirection.Descending;
            switch (sortField)
            {
                case "PRICE":
                    catalogItems = catalogItems.SortBy(o => o.Price, !sortDescending).ToList();
                    break;
                case "NAME":
                    catalogItems = catalogItems.SortBy(o => o.Name, !sortDescending).ToList();
                    break;
                default:
                    catalogItems = catalogItems.OrderBy(o => o.Price).ToList();
                    break;
            }
            //Paginazione
            catalogItems = catalogItems.Page(request.Query.Pagination, request.External).ToList();
            return new SmartStorageCatalog
            {
                TotalCount = totalCount,
                Values = catalogItems
            };
        }
        return null!;
    }

    private static void ParametersCheck(CatalogFilterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
    }
}
