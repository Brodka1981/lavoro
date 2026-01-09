using Aruba.CmpService.ResourceProvider.Common.ResourceQuery;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Swaases.Requests;

public class VirtualSwitchLinkSearchFilterRequest
{
    public string? SwaasId { get; set; }
    public string? UserId { get; set; }
    public ResourceQueryDefinition Query { get; set; }
    public string? ProjectId { get; set; }
    public bool External { get; set; }
}
