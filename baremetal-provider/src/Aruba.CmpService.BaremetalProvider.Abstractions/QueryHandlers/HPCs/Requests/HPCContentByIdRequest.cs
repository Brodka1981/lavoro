using Aruba.CmpService.ResourceProvider.Common.ResourceQuery;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.HPCs.Requests;
public class HPCContentByIdRequest : HPCByIdRequest
{
    public ResourceQueryDefinition? Query { get; set; }
}
