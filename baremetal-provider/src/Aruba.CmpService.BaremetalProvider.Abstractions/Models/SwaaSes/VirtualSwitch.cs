using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.SwaaSes;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class VirtualSwitch
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public IEnumerable<string> LocationCodes { get; set; } = new List<string>();
    public string? Location { get; set; }
    public VirtualSwitchStatuses? Status { get; set; }
    public DateTimeOffset? CreationDate { get; set; }
}
