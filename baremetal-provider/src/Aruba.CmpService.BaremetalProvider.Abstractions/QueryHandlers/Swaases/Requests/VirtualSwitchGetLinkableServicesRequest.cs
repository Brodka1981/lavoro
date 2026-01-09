namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Swaases.Requests;

public class VirtualSwitchGetLinkableServicesRequest
{
    public string? SwaasId { get; set; }
    public string? UserId { get; set; }
    public string? ProjectId { get; set; }
    public string? VirtualSwitchId { get; set; }
}
