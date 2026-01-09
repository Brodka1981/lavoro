using System.Diagnostics.CodeAnalysis;

// FIXME: @matteo.filippa check
namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.HPCs;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class HPCProperties : IResourceProperties
{
    public string? Name { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public DateTimeOffset? ActivationDate { get; set; }
    public bool AutoRenewEnabled { get; set; }
    public string? AutoRenewDeviceId { get; set; }
    public bool RenewAllowed { get; set; }
    public IEnumerable<string> Folders { get; set; } = new List<string>();
    public bool ShowVat { get; set; }
}
