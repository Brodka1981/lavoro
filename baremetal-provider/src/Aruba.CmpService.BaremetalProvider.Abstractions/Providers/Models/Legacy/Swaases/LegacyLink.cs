using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SwaaSes;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Internal;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Swaases;

public class LegacyLink
{
    public LegacyLinkConnectedService ConnectedService { get; set; } = new();
    public string? ConnectedServiceName { get; set; }
    public LegacyServiceType? ServiceType { get; set; }
    public string? ResourceId { get; set; }
    public string? ResourceName { get; set; }
    public string? ResourcePod { get; set; }
    public VirtualSwitchLinkStatuses? Status { get; set; }
    public object? VlanId { get; set; }
    public DateTimeOffset? CreatedOn { get; set; }
}
