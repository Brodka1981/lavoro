using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Internal;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacyAutoRenew
{
    public long? DeviceType { get; set; }
    public long? DeviceId { get; set; }
    public byte? FrequencyDuration { get; set; }
    public bool EnableAutoRenew { get; } = true;
    public IEnumerable<LegacyResourceRenew> Services { get; set; }
}
