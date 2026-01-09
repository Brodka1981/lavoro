using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
public abstract class BaseSearchFiltersRequest<TResource>
    where TResource : IResourceBase
{
    public string? UserId { get; set; }
    public ResourceQueryDefinition Query { get; set; }
    public string? ProjectId { get; set; }
    public bool External { get; set; }
}
