using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.TechnoRail.Models;


[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacyVlanId
{
    public string? Vlanid { get; set; }
}
