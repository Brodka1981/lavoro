using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Switches;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacySwitchListItem :
    LegacyResourceListItem
{
    public SwitchStatuses? Status { get; set; }
    public string? Model { get; set; }
    public string? ServerFarmCode { get; set; }
    public DateTime ActivationDate { get; set; }
}

