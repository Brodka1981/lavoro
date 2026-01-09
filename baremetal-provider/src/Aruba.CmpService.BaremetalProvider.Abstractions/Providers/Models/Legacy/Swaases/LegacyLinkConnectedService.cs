using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Internal;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Swaases;

public class LegacyLinkConnectedService
{
    public long Id { get; set; }
    public LegacyServiceType? ServiceType { get; set; }
}
