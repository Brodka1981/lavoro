using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SwaaSes;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Swaases.Requests;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.Filtering;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.Sorting;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Swaases;

public class VirtualSwitchGetByIdQueryHandler : IQueryHandler<VirtualSwitchGetByIdRequest, VirtualSwitch>
{
    private readonly ISwaasesService swaasesService;

    public VirtualSwitchGetByIdQueryHandler(ISwaasesService swaasesService)
    {
        this.swaasesService = swaasesService;
    }

    public async Task<VirtualSwitch> Handle(VirtualSwitchGetByIdRequest request)
    {
        ParametersCheck(request);

        var virtualSwitch = await swaasesService.GetVirtualSwitch(request).ConfigureAwait(false);
        return virtualSwitch.Value!;
    }

    private static void ParametersCheck(VirtualSwitchGetByIdRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
    }

    private static VirtualSwitchLinkList ApplyFilters(VirtualSwitchLinkList virtualSwitchList, FilterDefinitionsList? filters)
    {
        if (virtualSwitchList is null || !virtualSwitchList.Values.Any())
            return virtualSwitchList;

        var filteredVirtualSwitches = virtualSwitchList.Values;
        foreach (var filter in filters)
        {
            switch (filter)
            {
                case var f when f.IsFilterFor("status".AsField<VirtualSwitchLinkStatuses>(), op => op.Equal, out var arg):
                    filteredVirtualSwitches = filteredVirtualSwitches.Where(s => s.Status == arg).ToList();
                    break;

                case var f when f.IsFilterFor("serviceName".AsField<string>(), op => op.Equal, out var arg):
                    filteredVirtualSwitches = filteredVirtualSwitches.Where(s => s.LinkedServiceName?.Equals(arg, StringComparison.OrdinalIgnoreCase) ?? false).ToList();
                    break;

                case var f when f.IsFilterFor("typology".AsField<string>(), op => op.Equal, out var arg):
                    filteredVirtualSwitches = filteredVirtualSwitches.Where(s => s.LinkedServiceTypology?.Equals(arg, StringComparison.OrdinalIgnoreCase) ?? false).ToList();
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
        if (sorts is null)
        {
            return virtualSwitchLinkList;
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
            }
        }

        return new VirtualSwitchLinkList()
        {
            TotalCount = sortedFolders.Count,
            Values = sortedFolders
        };
    }
}
