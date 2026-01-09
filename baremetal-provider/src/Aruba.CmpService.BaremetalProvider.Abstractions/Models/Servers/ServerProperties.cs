using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.Servers;


[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class ServerProperties :
    IResourceProperties
{
    public string? Processor { get; set; }
    public string? OperatingSystem { get; set; }
    public string? Model { get; set; }
    public string? Ram { get; set; }
    public string? Gpu { get; set; }
    public string? Hdd { get; set; }
    public string? IpAddress { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public DateTimeOffset? ActivationDate { get; set; }
    public ServerPleskLicense? PleskLicense { get; set; }
    public decimal MonthlyUnitPrice { get; set; }
    public bool ShowVat { get; set; }
    public bool AutoRenewEnabled { get; set; }
    public bool AutoRenewAllowed { get; set; }
    public bool RenewAllowed { get; set; }
    public bool UpgradeAllowed { get; set; }
    public bool RenewUpgradeAllowed { get; set; }
    public string? AutoRenewDeviceId { get; set; }
    public long RenewMonths { get; set; }
    public string? ModelTypeCode { get; set; }
    public IEnumerable<string> Folders { get; set; } = new List<string>();
    public IEnumerable<ServerComponent> Components { get; set; } = new List<ServerComponent>();
    public string? OriginalName { get; set; }
    public string? ServerName { get; set; }
    public string? BundleProjectName { get; set; }
    public string? BundleCode { get; set; }
}