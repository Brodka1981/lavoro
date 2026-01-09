using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class BaremetalOptions
{
    public string? PurchaseUrl { get; set; }
    public string? RenewUrl { get; set; }
    public string? RenewAndUpgradeUrl { get; set; }
    public string? RenewAndUpgradeSmartStorageAndSwaasUrl { get; set; }
    public int? SddTimeLimit { get; set; }
    public string? CryptoKey { get; set; }
    public Dictionary<string, string> FrameworkAiGuides { get; init; } = new();
    public Dictionary<string, string> SmartStorageAiGuides { get; init; } = new();
}
