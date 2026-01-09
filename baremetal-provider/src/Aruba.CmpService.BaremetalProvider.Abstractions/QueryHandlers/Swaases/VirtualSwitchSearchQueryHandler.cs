using System.Linq;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SwaaSes;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Swaases.Requests;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.Filtering;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.Pagination;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.Sorting;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Swaases;

public class VirtualSwitchSearchQueryHandler : IQueryHandler<VirtualSwitchSearchFilterRequest, VirtualSwitchList>
{
    private readonly ISwaasesService swaasesService;

    public VirtualSwitchSearchQueryHandler(ISwaasesService swaasesService)
    {
        this.swaasesService = swaasesService;
    }

    public async Task<VirtualSwitchList> Handle(VirtualSwitchSearchFilterRequest request)
    {
        ParametersCheck(request);

        var virtualSwitches = await swaasesService.GetVirtualSwitches(request).ConfigureAwait(false);
        if (virtualSwitches.Errors.Any() || virtualSwitches.Value is null)
        {
            return virtualSwitches.Value!;
        }
        var filteredresult = ApplySort(ApplyFilters(virtualSwitches.Value, request.Query?.Filters), request.Query?.Sorts);

        var paginationResult = filteredresult.Values.SetSearchPagination(request.Query?.Pagination ?? new PagingDefinition(), request.External).ToList();

        return new VirtualSwitchList()
        {
            TotalCount = filteredresult.TotalCount,
            Values = paginationResult,
        };
    }

    private static void ParametersCheck(VirtualSwitchSearchFilterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
    }

    private static VirtualSwitchList ApplyFilters(VirtualSwitchList virtualSwitchList, FilterDefinitionsList? filters)
    {
        if (virtualSwitchList is null || !virtualSwitchList.Values.Any())
            return virtualSwitchList;

        var filteredVirtualSwitches = virtualSwitchList.Values;
        foreach (var filter in filters)
        {
            switch (filter)
            {
                case var f when f.IsFilterFor("status".AsField<string>(), op => op.Equal, out var arg):
                    filteredVirtualSwitches = filteredVirtualSwitches.Where(s => s.Status == Enum.Parse<VirtualSwitchStatuses>(arg)).ToList();
                    break;

                case var f when f.IsFilterFor("location".AsField<string>(), op => op.Equal, out var arg):
                    filteredVirtualSwitches = filteredVirtualSwitches.Where(s => (s.Location?.Equals(arg, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
                    break;

                case var f when f.IsFilterFor("fulltextsearch".AsField<string>(), op => op.Equal, out var arg):
                    filteredVirtualSwitches = filteredVirtualSwitches.Where(s => (s.Name?.Contains(arg, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
                    break;
            }
        }
        return new VirtualSwitchList()
        {
            TotalCount = filteredVirtualSwitches.Count,
            Values = filteredVirtualSwitches
        };
    }

    private static VirtualSwitchList ApplySort(VirtualSwitchList virtualSwitchList, SortDefinitionsList? sorts)
    {
        if (sorts is null || !sorts.Any())
        {
            // Default sort by Descending crationDate
            return new VirtualSwitchList()
            {
                TotalCount = virtualSwitchList.Values.Count,
                Values = virtualSwitchList.Values.OrderByDescending(v => v.CreationDate).ToList()
            };
        }

        var sortedFolders = virtualSwitchList.Values;
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

                case var s when s.IsSortFor("name"):
                    sortedFolders = descending
                        ? sortedFolders.OrderByDescending(s => s.Name, StringComparer.OrdinalIgnoreCase).ToList()
                        : sortedFolders.OrderBy(s => s.Name, StringComparer.OrdinalIgnoreCase).ToList();
                    break;

                case var s when s.IsSortFor("location"):
                    sortedFolders = descending
                        ? sortedFolders.OrderByDescending(s => s.Location).ToList()
                        : sortedFolders.OrderBy(s => s.Location).ToList();
                    break;

                case var s when s.IsSortFor("status"):
                    sortedFolders = descending
                        ? sortedFolders.OrderByDescending(s => s.Status).ToList()
                        : sortedFolders.OrderBy(s => s.Status).ToList();
                    break;
            }
        }

        return new VirtualSwitchList()
        {
            TotalCount = sortedFolders.Count,
            Values = sortedFolders
        };
    }
}
