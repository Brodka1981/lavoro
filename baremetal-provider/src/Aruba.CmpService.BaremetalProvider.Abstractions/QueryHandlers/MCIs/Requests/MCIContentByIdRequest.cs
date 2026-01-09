using Aruba.CmpService.ResourceProvider.Common.ResourceQuery;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.MCIs.Requests;
public class MCIContentByIdRequest : MCIByIdRequest
{
    public ResourceQueryDefinition? Query { get; set; }
}
