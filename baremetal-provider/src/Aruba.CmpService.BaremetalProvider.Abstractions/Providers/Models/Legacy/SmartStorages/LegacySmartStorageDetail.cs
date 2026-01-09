using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.SmartStorages;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacySmartStorageDetail :
    LegacyResourceDetail
{
    public SmartStoragesStatuses? Status { get; set; }
    public bool HasReplica { get; set; }
    public string? Package { get; set; }
    public string? Username { get; set; }
    public string? ServerAddress { get; set; }
    public string? SambaAddress { get; set; }
    public bool UpgradeAllowed { get; set; }
    public string? CustomName { get; set; }
    public bool FirstSetupDone { get; set; }
    public LegacySmartStorageInfo SmartStorageInfo { get; set; } = new();
    public string? Model { get; set; }
    public string? IpAddress { get; set; }
    public string? MaskSuffix { get; set; }
    public int? MaxSnapshots { get; set; }
    public int? MaxSmartFolders { get; set; }
}
