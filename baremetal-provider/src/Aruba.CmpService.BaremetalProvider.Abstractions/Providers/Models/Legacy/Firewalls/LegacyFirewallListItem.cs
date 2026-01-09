using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Firewalls;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacyFirewallListItem :
    LegacyResourceListItem
{
    public FirewallStatuses? Status { get; set; }
    public string? Model { get; set; }
    public string? ConfigurationMode { get; set; }
    public DateTime ActivationDate { get; set; }
    public string? ServerFarmCode { get; set; }
}