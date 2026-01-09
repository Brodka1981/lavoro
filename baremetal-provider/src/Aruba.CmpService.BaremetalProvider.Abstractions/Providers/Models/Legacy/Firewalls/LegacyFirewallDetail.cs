using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Firewalls;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacyFirewallDetail :
    LegacyResourceDetail
{
    public FirewallStatuses? Status { get; set; }
    public string? Model { get; set; }
    public string? IpAddress { get; set; }
    public string? ConfigurationMode { get; set; }
    public string? ServerFarmCode { get; set; }
    public string? CustomName { get; set; }
    public string? BundleProjectName { get; set; }
    public string? BundleCode { get; set; }
}