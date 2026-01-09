using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Servers;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacyServerDetail :
    LegacyResourceDetail
{
    public ServerStatuses? Status { get; set; }
    public string? Model { get; set; }
    public string? CPU { get; set; }
    public string? GPU { get; set; }
    public string? RAM { get; set; }
    public string? OS { get; set; }
    public string? IpAddress { get; set; }
    public string? ServerFarmCode { get; set; }
    public string? Hdd { get; set; }
    public IEnumerable<LegacyServerPleskLicense> PleskLicensesInfo { get; set; } = new List<LegacyServerPleskLicense>();
    public string? CustomName { get; set; }
    public bool UpgradeAllowed { get; set; }
    public bool RenewUpgradeAllowed { get; set; }
    public string? ModelTypeCode { get; set; }
    public string? BundleProjectName {  get; set; }
    public string? BundleCode { get; set; }
}

