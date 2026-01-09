namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Swaases;

public class LegacyVirtualSwitchLink
{
    public LegacyLink Resource { get; set; } = new();
    public LegacyVirtualSwitch VirtualNetwork { get; set; } = new();
}
