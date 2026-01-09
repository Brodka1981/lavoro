using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Swaases;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacySwaasListItem :
    LegacyResourceListItem
{
    public SwaasStatuses? Status { get; set; }    
    public string? Model { get; set; }
    public string? ServerFarmCode { get; set; }
    public DateTime ActivationDate { get; set; }
}

