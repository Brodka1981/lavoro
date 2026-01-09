using Aruba.CmpService.ResourceProvider.Common.ResourceQuery;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Servers.Requests;

public class ServerIpAddressesFilterRequest
{
    public long ResourceId { get; set; }
    public string? UserId { get; set; }
    public ResourceQueryDefinition Query { get; set; }
    public string? ProjectId { get; set; }
    public bool External { get; set; }

}
