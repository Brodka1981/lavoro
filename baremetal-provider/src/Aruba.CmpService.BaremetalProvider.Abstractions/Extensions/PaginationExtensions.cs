using System.Web;
using Aruba.CmpService.ResourceProvider.Common.Dtos.Response;
using Microsoft.AspNetCore.Http;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;

public static class PaginationExtensions
{
    public static ListResponseDto<T> FillPaginationData<T>(this ListResponseDto<T> pagination, HttpRequest httpRequest)
    {
        return pagination.FillPaginationData(httpRequest.Scheme, httpRequest.Host.ToString(), httpRequest.Path, httpRequest.QueryString.ToString());
    }

    public static ListResponseDto<T> FillPaginationData<T>(this ListResponseDto<T> pagination, string? scheme, string? host, string? path, string? queryString)
    {
        ArgumentNullException.ThrowIfNull(pagination);
        ArgumentNullException.ThrowIfNull(path);

        var queryValues = HttpUtility.ParseQueryString(queryString ?? string.Empty);

        var offsetValues = queryValues?.GetValues("offset"); // n elem da skippare
        var limitValues = queryValues?.GetValues("limit"); // n elem per pag

        if (pagination.TotalCount == 0
            || string.IsNullOrWhiteSpace(queryString)
            || offsetValues is null
            || limitValues is null)
            return pagination;

        var offset = int.Parse(offsetValues.First());
        var limit = int.Parse(limitValues.First());

        var maxOffset = pagination.TotalCount <= limit
            ? 0
            : Math.Floor((decimal)(pagination.TotalCount / limit)) * limit;

        if (maxOffset >= pagination.TotalCount)
        {
            maxOffset = maxOffset - limit;
        }

        pagination.Self = $"{scheme}://{host}{path}?{queryValues}";

        // LAST
        if (offset != maxOffset)
        {
            queryValues.Set("offset", $"{maxOffset}");
            pagination.Last = $"{scheme}://{host}{path}?{queryValues}";
        }

        // FIRST
        if (offset != 0)
        {
            queryValues.Set("offset", "0");
            pagination.First = $"{scheme}://{host}{path}?{queryValues}";
        }

        // NEXT
        var nextOffset = offset + limit;
        if (nextOffset <= maxOffset)
        {
            queryValues.Set("offset", $"{nextOffset}");
            pagination.Next = $"{scheme}://{host}{path}?{queryValues}";
        }

        // PREV
        var previousOffset = offset - limit;
        if (previousOffset >= 0)
        {
            queryValues.Set("offset", $"{previousOffset}");
            pagination.Prev = $"{scheme}://{host}{path}?{queryValues}";
        }

        return pagination;
    }
}
