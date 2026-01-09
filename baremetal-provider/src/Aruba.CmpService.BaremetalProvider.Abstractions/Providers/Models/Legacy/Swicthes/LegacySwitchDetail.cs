using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Switches;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacySwitchDetail :
    LegacyResourceDetail
{
    public SwitchStatuses? Status { get; set; }
    public string? Model { get; set; }
    public string? ServerFarmCode { get; set; }
    public string? CustomName { get; set; }
}

