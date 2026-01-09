using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SwaaSes;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Swaases;

public class LegacyVirtualSwitch
{
    public string? VirtualNetworkId { get; set; }
    public string? FriendlyName { get; set; }
    public string? Region { get; set; }
    public DateTimeOffset? CreateDate { get; set; }
    public DateTimeOffset? DeleteDate { get; set; }
    public VirtualSwitchStatuses? Status { get; set; }
}
