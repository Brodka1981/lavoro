using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.SmartStorages;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class SmartStorageProperties :
    IResourceProperties
{
    public string? Package { get; set; }
    public bool Replica { get; set; }
    public string? Username { get; set; }
    public string? Server { get; set; }
    public string? Samba { get; set; }
    public DateTimeOffset? ActivationDate { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public decimal MonthlyUnitPrice { get; set; }
    public bool ShowVat { get; set; }
    public bool AutoRenewEnabled { get; set; }
    public bool RenewAllowed { get; set; }
    public bool UpgradeAllowed { get; set; }
    public bool IsFirstSetupDone { get; set; }
    public string? AutoRenewDeviceId { get; set; }
    public long? RenewMonths { get; set; }
    public IEnumerable<string> Folders { get; set; } = new List<string>();
}
