using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.Firewalls;
[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class VlanID
{
    public string? Vlanid {  get; set; }
}
