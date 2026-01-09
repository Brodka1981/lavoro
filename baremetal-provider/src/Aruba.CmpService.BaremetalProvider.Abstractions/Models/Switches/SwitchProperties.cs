using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.Switches;


[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class SwitchProperties :
    IResourceProperties
{
    public string? Admin { get; set; }
    public string? Model { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public DateTimeOffset? ActivationDate { get; set; }
    public decimal MonthlyUnitPrice { get; set; }
    public bool ShowVat { get; set; }
    public bool AutoRenewEnabled { get; set; }
    public bool AutoRenewAllowed { get; set; }
    public string? AutoRenewDeviceId { get; set; }
    public long? RenewMonths { get; set; }
    public bool RenewAllowed { get; set; }
    public IEnumerable<string> Folders { get; set; } = new List<string>();

}