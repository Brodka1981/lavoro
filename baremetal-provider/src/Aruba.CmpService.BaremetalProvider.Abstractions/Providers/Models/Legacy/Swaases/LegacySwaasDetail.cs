using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Swaases;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacySwaasDetail :
    LegacyResourceDetail
{
    public SwaasStatuses? Status { get; set; }
    public string? Model { get; set; }
    public bool Reply { get; set; }
    public string? ServerFarmCode { get; set; }
    public bool UpgradeAllowed { get; set; }
    public bool RenewUpgradeAllowed { get; set; }
    public string? CustomName { get; set; }
    public int? MaxAvailability { get; set; }

}

