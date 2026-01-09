using System.Globalization;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.Sorting;
using Aruba.Extensions.Reflection;
using Microsoft.AspNetCore.WebUtilities;
using Throw;

namespace Aruba.CmpService.BaremetalProvider.HttpProviders.Extensions;
public static class QueryFilterExtensions
{
    internal static LegacySearchFilters SetSort(this LegacySearchFilters request, IDictionary<string, string> fieldMapping, string? defaultSortField = "activationDate", SortDirection defaultSortDirection = SortDirection.Descending)
    {
        SortDefinitionsList sortDefinitions = new SortDefinitionsList();

        var requestSortField = request.Query.Sorts.FirstOrDefault()?.FieldName;
        if (!string.IsNullOrWhiteSpace(requestSortField))
        {
            var sortDirection = request.Query.Sorts.First().Direction;
            if (fieldMapping.ContainsKey(requestSortField))
            {
                sortDefinitions.Add(SortDefinition.Create(fieldMapping[requestSortField], sortDirection));
            }
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(defaultSortField))
            {
                sortDefinitions.Add(SortDefinition.Create(defaultSortField, defaultSortDirection));
            }
        }

        request.Query.Sorts = sortDefinitions;
        return request;
    }
    public static string ToIpAddressQueryString(this LegacySearchFilters request, string baseUrl)
    {

        request.ThrowIfNull();
        var newSortDefintionList = new SortDefinitionsList();
        if (request.Query?.Sorts?.Any() ?? false)
        {
            var sortDirection = request.Query.Sorts.First().Direction;
            if ((request.Query.Sorts.First().FieldName?.Equals("description", StringComparison.OrdinalIgnoreCase)) ?? false)
            {
                newSortDefintionList.Add(SortDefinition.Create("customName", sortDirection));
            }
        }
        request.Query.Sorts = newSortDefintionList;

        return request.ToQueryString(baseUrl);
    }

    internal static string ToQueryString(this LegacySearchFilters request, string baseUrl)
    {
        var offset = Math.Max(request.Query.Pagination?.Offset ?? 0, 0);
        var limit = Math.Min(request.Query.Pagination?.Limit ?? ushort.MaxValue, ushort.MaxValue);
        if (request.External)
        {
            limit = Math.Min(limit, 100);
        }
        var fieldFilter = request.Query.Filters.FirstOrDefault(f => string.Equals(f.FieldName, "fulltextsearch", StringComparison.OrdinalIgnoreCase))?.Argument.As<string?>();

        var parameters = new Dictionary<string, string?>(StringComparer.Ordinal);
        parameters.AddRange(request.Query.Filters.Where(w => !w.FieldName.Equals("fulltextsearch", StringComparison.OrdinalIgnoreCase)).ToDictionary(k => k.FieldName, v => v.Argument.RawValue));
        parameters.Add("offset", offset.ToString(CultureInfo.InvariantCulture));
        parameters.Add("pageSize", limit.ToString(CultureInfo.InvariantCulture));
        if (!string.IsNullOrWhiteSpace(fieldFilter))
        {
            parameters.Add("searchString", fieldFilter);
        }
        if (request.Query.Sorts.Any())
        {
            parameters.Add("sortBy", request.Query.Sorts.First().FieldName);
            parameters.Add("sortMode", request.Query.Sorts.First().Direction == SortDirection.Ascending ? "asc" : "desc");
        }

        return QueryHelpers.AddQueryString(baseUrl, parameters);
    }
}
