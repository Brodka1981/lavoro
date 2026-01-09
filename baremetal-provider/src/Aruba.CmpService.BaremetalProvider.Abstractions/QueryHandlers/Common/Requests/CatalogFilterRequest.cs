using Aruba.CmpService.ResourceProvider.Common.ResourceQuery;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
public class CatalogFilterRequest
{
    public ResourceQueryDefinition Query { get; set; }
    public bool External { get; set; }
    public string? Language { get; set; }
}
