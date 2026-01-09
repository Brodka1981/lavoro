using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SwaaSes;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Swaases.Requests;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.Filtering;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.Pagination;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.Sorting;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Swaases;

public class VirtualSwitchLinkSearchQueryHandler : IQueryHandler<VirtualSwitchLinkSearchFilterRequest, VirtualSwitchLinkList>
{
    private readonly ISwaasesService swaasesService;

    public VirtualSwitchLinkSearchQueryHandler(ISwaasesService swaasesService)
    {
        this.swaasesService = swaasesService;
    }

    public async Task<VirtualSwitchLinkList> Handle(VirtualSwitchLinkSearchFilterRequest request)
    {
        ParametersCheck(request);

        var virtualSwitches = await swaasesService.GetVirtualSwitchLinks(request).ConfigureAwait(false);

        var filteredresult = ApplySort(ApplyFilters(virtualSwitches.Value, request.Query?.Filters), request.Query?.Sorts);

        var paginationResult = filteredresult.Values.SetSearchPagination(request.Query?.Pagination ?? new PagingDefinition(), request.External).ToList();

        return new VirtualSwitchLinkList()
        {
            TotalCount = filteredresult.TotalCount,
            Values = paginationResult,
        };
    }

    private static void ParametersCheck(VirtualSwitchLinkSearchFilterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
    }

    private static VirtualSwitchLinkList ApplyFilters(VirtualSwitchLinkList virtualSwitchList, FilterDefinitionsList? filters)
    {

        if (virtualSwitchList is null || virtualSwitchList.Values.Count == 0)
            return virtualSwitchList ?? new VirtualSwitchLinkList();

        var filteredVirtualSwitches = virtualSwitchList.Values;
        foreach (var filter in filters ?? new FilterDefinitionsList())
        {
            switch (filter)
            {
                case var f when f.IsFilterFor("status".AsField<string>(), op => op.Equal, out var arg):
                    filteredVirtualSwitches = filteredVirtualSwitches.Where(s => s.Status == Enum.Parse<VirtualSwitchLinkStatuses>(arg)).ToList();
                    break;

                case var f when f.IsFilterFor("name".AsField<string>(), op => op.Equal, out var arg):
                    filteredVirtualSwitches = filteredVirtualSwitches.Where(s => s.LinkedServiceName?.Equals(arg, StringComparison.OrdinalIgnoreCase) ?? false).ToList();
                    break;

                case var f when f.IsFilterFor("typology".AsField<string>(), op => op.Equal, out var arg):
                    filteredVirtualSwitches = filteredVirtualSwitches.Where(s => s.LinkedServiceTypology?.Equals(arg, StringComparison.OrdinalIgnoreCase) ?? false).ToList();
                    break;

                case var f when f.IsFilterFor("fulltextSearch".AsField<string>(), op => op.Equal, out var arg):
                    filteredVirtualSwitches = filteredVirtualSwitches.Where(s => (s.LinkedServiceName?.Contains(arg, StringComparison.OrdinalIgnoreCase) ?? false)
                                                                              || (s.VirtualSwitchName?.Contains(arg, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
                    break;
            }
        }
        return new VirtualSwitchLinkList()
        {
            TotalCount = filteredVirtualSwitches.Count,
            Values = filteredVirtualSwitches
        };
    }

    private static VirtualSwitchLinkList ApplySort(VirtualSwitchLinkList virtualSwitchLinkList, SortDefinitionsList? sorts)
    {
        if (sorts is null || !sorts.Any())
        {
            // Default sort by Descending crationDate
            return new VirtualSwitchLinkList()
            {
                TotalCount = virtualSwitchLinkList.Values.Count,
                Values = virtualSwitchLinkList.Values.OrderByDescending(v => v.CreationDate).ToList()
            };
        }

        var sortedFolders = virtualSwitchLinkList.Values;
        foreach (var sort in sorts)
        {
            var descending = sort.Direction == SortDirection.Descending;

            switch (sort)
            {
                case var s when s.IsSortFor("id"):
                    sortedFolders = descending
                        ? sortedFolders.OrderByDescending(s => s.Id, StringComparer.OrdinalIgnoreCase).ToList()
                        : sortedFolders.OrderBy(s => s.Id, StringComparer.OrdinalIgnoreCase).ToList();
                    break;

                case var s when s.IsSortFor("status"):
                    sortedFolders = descending
                        ? sortedFolders.OrderByDescending(s => s.Status).ToList()
                        : sortedFolders.OrderBy(s => s.Status).ToList();
                    break;

                case var s when s.IsSortFor("virtualSwitchName"):
                    sortedFolders = descending
                        ? sortedFolders.OrderByDescending(s => s.VirtualSwitchName, StringComparer.OrdinalIgnoreCase).ToList()
                        : sortedFolders.OrderBy(s => s.VirtualSwitchName, StringComparer.OrdinalIgnoreCase).ToList();
                    break;

                case var s when s.IsSortFor("typology"):
                    sortedFolders = descending
                        ? sortedFolders.OrderByDescending(s => s.LinkedServiceTypology, StringComparer.OrdinalIgnoreCase).ToList()
                        : sortedFolders.OrderBy(s => s.LinkedServiceTypology, StringComparer.OrdinalIgnoreCase).ToList();
                    break;

                case var s when s.IsSortFor("name"):
                    sortedFolders = descending
                        ? sortedFolders.OrderByDescending(s => s.LinkedServiceName, StringComparer.OrdinalIgnoreCase).ToList()
                        : sortedFolders.OrderBy(s => s.LinkedServiceName, StringComparer.OrdinalIgnoreCase).ToList();
                    break;

                case var s when s.IsSortFor("vlanId"):
                    sortedFolders = descending
                        ? sortedFolders.OrderByDescending(s => s.VlanId, StringComparer.OrdinalIgnoreCase).ToList()
                        : sortedFolders.OrderBy(s => s.VlanId, StringComparer.OrdinalIgnoreCase).ToList();
                    break;
            }
        }

        return new VirtualSwitchLinkList()
        {
            TotalCount = sortedFolders.Count,
            Values = sortedFolders
        };
    }
}
