using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class ResourceUpsertAutomaticRenew :
    ResourceDeleteAutomaticRenew
{
    public override bool EnableAutoRenew { get; } = true;
    public byte? FrequencyDuration { get; set; }
    public long? DeviceId { get; set; }
    public long? DeviceType { get; set; }
}
