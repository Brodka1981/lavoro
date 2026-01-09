using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.Pagination;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
internal static class IQueryHandlerExtensions
{
    private const int MaxPaginationLimitForExternalSource = 100;

    internal static ICollection<TModel> SetSearchPagination<TModel>(this ICollection<TModel> search, PagingDefinition pagination, bool external)
    {
        return search.AsQueryable().SetSearchPagination(pagination, external).ToCollection();
    }

    internal static IEnumerable<TModel> SetSearchPagination<TModel>(this IEnumerable<TModel> search, PagingDefinition pagination, bool external)
    {
        ArgumentNullException.ThrowIfNull(search);

        var offset = pagination?.Offset;
        var limit = pagination?.Limit;

        if (external)
        {
            limit = Math.Min(limit ?? int.MaxValue, MaxPaginationLimitForExternalSource);
        }

        if (offset.HasValue)
        {
            search = search.Skip(offset.Value);
        }
        if (limit.HasValue)
        {
            search = search.Take(limit.Value);
        }

        return search;
    }
}
